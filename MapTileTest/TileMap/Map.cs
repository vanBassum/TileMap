using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Security;

namespace TileMap
{
    public class Map
    {
        public delegate void DrawObjects(Graphics g, CoordConverter coordConverter);
        public event DrawObjects OnDrawObjects;
        public event Action OnMapLoaded;
        public Panel viewPort { get; set; } = new Panel();
        public Coordinate centerCoordinate { get; set; } = new Coordinate(0,0);
        public int Zoomlevel { get; set; } = 0;
        public int MaxZoomLevel { get => mapSettings.Zoomlevels- 1; }
        private PictureBox pictureBox = new PictureBox();
        private Thread updateImageThread;
        private RenderedImage activeImage;
        private AutoResetEvent requestImage = new AutoResetEvent(false);
        private delegate void ImageRenderedHandle(RenderedImage img);
        private event ImageRenderedHandle OnImageRendered;
        private string cacheFolder = "";
        private bool mapRendered = false;
        private MapSettings mapSettings;
        private bool terminateThread = false;
        private CoordConverter coordConverter;

        public Map(string mapFile)
        {
            cacheFolder = Path.GetDirectoryName(mapFile);
            mapSettings = new MapSettings();

            //mapSettings.Save(mapFile);

            using (Stream stream = File.Open(mapFile, FileMode.Open, FileAccess.Read ))
                mapSettings.Load(stream);
            


            coordConverter = new CoordConverter();
            coordConverter.Calibrate(mapSettings.CalibrationPoints);
            OnMapLoaded?.Invoke();
            viewPort.Controls.Add(pictureBox);
            updateImageThread = new Thread(UpdateImage);
            updateImageThread.Start();
            OnImageRendered += Map_OnImageRendered;
            
        }
        
        public void Dispose()
        {
            terminateThread = true;
        }

        private void Map_OnImageRendered(RenderedImage img)
        {
            activeImage = img.Clone() as RenderedImage;
            if (activeImage.Image == null)
                return;

            pictureBox.InvokeIfRequired(pb => 
            {
                pictureBox.BackgroundImage = activeImage.Image;
                pictureBox.Size = activeImage.Image.Size;
            });

            mapRendered = true;
        }

        Image img;
        public void Render()
        {
            //Move the picturebox according to scale.
            if(!mapRendered)
            {
                requestImage.Set();
                return;
            }

            Coordinate move = activeImage.TopLeft - centerCoordinate;
            Point mv = coordConverter.Convert(move, Zoomlevel);
            mv.X += viewPort.Width / 2;
            mv.Y += viewPort.Height / 2;
            pictureBox.Location = mv;

            if (img != null)
                img.Dispose();

            img = new Bitmap(pictureBox.Width, pictureBox.Height);

            using (Graphics g = Graphics.FromImage(img))
            {
                Point pt = coordConverter.Convert(activeImage.TopLeft, Zoomlevel);
                g.TranslateTransform(-pt.X, -pt.Y);
                OnDrawObjects?.Invoke(g, coordConverter);
                g.TranslateTransform(0, 0);
                g.Dispose();
            }
            
            pictureBox.Image = img;
            

            //Check if update is needed
            int a = -mv.X;
            int b = -mv.Y;
            int c = pictureBox.Width - a - viewPort.Width;
            int d = pictureBox.Height - b - viewPort.Height;
            int e = mapSettings.Tilesize.Width;

            if (a < e || b < e || c < e || d < e)
            {
                requestImage.Set();
            }
        }

        private void UpdateImage()
        {
            RenderedImage bufferImage = new RenderedImage();
            while(!terminateThread)
            {
                //Wait for start
                if (requestImage.WaitOne(100))
                {
                    Point center = coordConverter.Convert(centerCoordinate, Zoomlevel);


                    int firstTileX = ((int)Math.Floor((double)(center.X - viewPort.Width / 2) / (double)mapSettings.Tilesize.Width)) - 1;
                    int firstTileY = ((int)Math.Floor((double)(center.Y - viewPort.Height / 2) / (double)mapSettings.Tilesize.Height)) - 1;

                    int lastTileX = ((int)Math.Ceiling((double)(center.X + viewPort.Width / 2) / (double)mapSettings.Tilesize.Width)) + 1;
                    int lastTileY = ((int)Math.Ceiling((double)(center.Y + viewPort.Height / 2) / (double)mapSettings.Tilesize.Height)) + 1;

                    int tilesX = lastTileX - firstTileX;
                    int tilesY = lastTileY - firstTileY;

                    bufferImage.TopLeft = coordConverter.Convert(new Point(firstTileX * mapSettings.Tilesize.Width, firstTileY * mapSettings.Tilesize.Height), Zoomlevel);
                    bufferImage.Image?.Dispose();
                    bufferImage.Image = new Bitmap(tilesX * mapSettings.Tilesize.Width, tilesY * mapSettings.Tilesize.Height);
                    Graphics g = Graphics.FromImage(bufferImage.Image);

                    for(int x=0; x<tilesX; x++)
                    {
                        for(int y=0; y<tilesY; y++)
                        {
                            int tX = x + firstTileX;
                            int tY = y + firstTileY;
                            string tileName = string.Format("{0}\\{1}-{2}.png", Zoomlevel, tX, tY);
                            string tileFile = Path.Combine(cacheFolder, "Tiles" , tileName);

                            Point drawPos = new Point(x * mapSettings.Tilesize.Width, y * mapSettings.Tilesize.Height);

                            if(File.Exists(tileFile))
                            {
                                using (Image img = new Bitmap(tileFile))
                                    g.DrawImageUnscaled(img, drawPos);
                            }
                            else
                            {
                                g.FillRectangle(Brushes.LightGray, new Rectangle(drawPos, new Size(mapSettings.Tilesize.Width, mapSettings.Tilesize.Height)));
                            }

                        }
                    }

                    g.Dispose();

                    //Update the renderedimage on ui
                    EXT.RaiseEventOnUIThread(OnImageRendered, new object[] { bufferImage });
                }
            }
        }       
    }
}

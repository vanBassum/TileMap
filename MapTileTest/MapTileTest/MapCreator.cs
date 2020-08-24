using FRMLib.Scope;
using FRMLib.TileImage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapTileTest
{
    public partial class MapCreator : Form
    {
        public MapCreator()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Trim('\"');
            string dir = Path.Combine(Path.GetDirectoryName(textBox1.Text), Path.GetFileNameWithoutExtension(textBox1.Text));
            Directory.CreateDirectory(dir);

            Image img = Image.FromFile(textBox1.Text);


            Map tc = new Map();



            int niveau = 0;

            while(img.Width > numericUpDown1.Value && img.Height > numericUpDown2.Value)
            {
                TileSet ts = new TileSet();
                ts.Muliplier = 1 << niveau;

                for (int y = 0; y < Math.Ceiling(img.Height / numericUpDown2.Value); y++)
                {
                    for (int x = 0; x < Math.Ceiling(img.Width / numericUpDown1.Value); x++)
                    {
                        string filename = $"Tiles\\{niveau}\\{y},{x}.png";
                        Tile tile = new Tile(); 
                        tile.ImageFile = Path.Combine(dir, filename);
                        tile.Size = new SizeD((double)numericUpDown1.Value, (double)numericUpDown2.Value);
                        tile.Position = (new V2D(x, y) * tile.Size);
                        Image timg = new Bitmap((int)tile.Size.Width, (int)tile.Size.Height);
                        var graphics = Graphics.FromImage(timg);
                        graphics.DrawImage(img, new Rectangle(new Point(0,0), (Size)(V2D)tile.Size), new Rectangle((Point)(V2D)tile.Position, (Size)(V2D)tile.Size), GraphicsUnit.Pixel);
                        graphics.Dispose();
                        Directory.CreateDirectory(Path.GetDirectoryName(tile.ImageFile));
                        timg.Save(tile.ImageFile);
                        timg.Dispose();
                        ts.Tiles.Add(tile);
                    }
                }

                tc.TileSets.Add(ts);
                niveau++;
                img = ResizeImage(img, img.Width / 2, img.Height / 2);
            }

            tc.Save(Path.Combine(dir, "map.json"));
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}

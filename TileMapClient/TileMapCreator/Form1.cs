using STDLib.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TileMapLib;

namespace TileMapCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Settings.Load();
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Trim('\"');
            //Create a tempoary dir to store files.
            string tempFolder = PrepTempFolder(Settings.TempFolder);
            Image img = Image.FromFile(textBox1.Text);
            Map map = new Map();
            map.GUID = Guid.NewGuid();
            map.Name = textBox2.Text;
            int niveau = 0;

            while (img.Width > numericUpDown1.Value && img.Height > numericUpDown2.Value)
            {
                TileSet ts = new TileSet();
                ts.Muliplier = 1 << niveau;

                for (int y = 0; y < Math.Ceiling(img.Height / numericUpDown2.Value); y++)
                {
                    for (int x = 0; x < Math.Ceiling(img.Width / numericUpDown1.Value); x++)
                    {
                        string filename = $"Tiles\\{niveau}\\{y},{x}.png";
                        Tile tile = new Tile();
                        tile.ImageFile = filename;
                        tile.Size = new V2D((double)numericUpDown1.Value, (double)numericUpDown2.Value);
                        tile.Position = (new V2D(x, y) * tile.Size);
                        Image timg = new Bitmap((int)numericUpDown1.Value, (int)numericUpDown2.Value);
                        var graphics = Graphics.FromImage(timg);
                        graphics.DrawImage(img, new Rectangle(new Point(0, 0), (Size)(V2D)tile.Size), new Rectangle((Point)(V2D)tile.Position, (Size)(V2D)tile.Size), GraphicsUnit.Pixel);
                        graphics.Dispose();
                        Directory.CreateDirectory(Path.GetDirectoryName(tile.ImageFile));
                        string fullPath = Path.Combine(tempFolder, tile.ImageFile);
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        timg.Save(fullPath);
                        timg.Dispose();
                        ts.Tiles.Add(tile);
                    }
                }

                map.TileSets.Add(ts);
                niveau++;
                img = ResizeImage(img, img.Width / 2, img.Height / 2);
            }

            map.Save(Path.Combine(tempFolder, "map.json"));

            ZipFile.CreateFromDirectory(tempFolder, Path.Combine(Path.GetDirectoryName(textBox1.Text), Path.GetFileNameWithoutExtension(textBox1.Text) + ".tilemap"));

            Directory.Delete(tempFolder, true);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Trim('\"');
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

        /// <summary>
        /// Creates a new directory in path and makes sure it exists and is empty.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static string PrepTempFolder(string path)
        {
            string tempFolder;
            while (Directory.Exists(tempFolder = Path.Combine(Settings.TempFolder, GetRandomHexNumber(8)))) ;
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }

        static Random random = new Random();
        static string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

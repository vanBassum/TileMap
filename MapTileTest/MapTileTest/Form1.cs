using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TileMap;
using Datasave;
using System.Net;
using System.IO;

namespace MapTileTest
{
    public partial class Form1 : Form
    {
        //JSONSerializer json = new JSONSerializer();
        //Settings settings;
        //MapProvider mapProvider;
        Map map;
        //string cacheFolder = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string assemblyName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);

            map = new Map(Path.Combine(appdataFolder, assemblyName, "Cache", "Conan exiles\\Settings.json"));
            map.OnDrawObjects += Map_OnDrawObjects;
            map.OnMapLoaded += Map_OnMapLoaded;
            panel1.Controls.Add(map.viewPort);
            map.viewPort.Dock = DockStyle.Fill;
            map.viewPort.BorderStyle = BorderStyle.FixedSingle;
            timer1.Start();
        }

        private void Map_OnMapLoaded()
        {
            numericUpDown1.Maximum = map.MaxZoomLevel;
        }

        private void Map_OnDrawObjects(Graphics g, CoordConverter coordConverter)
        {
            DrawCross(g, coordConverter, new Coordinate(0, 0));
            DrawCross(g, coordConverter, new Coordinate(32, 32));
        }

        private void DrawCross(Graphics g, CoordConverter ccv, Coordinate coord)
        {

            Point p = ccv.Convert(coord, map.Zoomlevel);

            g.DrawLine(Pens.Red, p.X - 5, p.Y - 5, p.X + 5, p.Y + 5);
            g.DrawLine(Pens.Red, p.X - 5, p.Y + 5, p.X + 5, p.Y - 5);
        }




        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            map.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            map.Render();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            map.Zoomlevel = (int)numericUpDown1.Value;
        }
    }

    

    
    /*

    [Serializable]
    public class Settings
    {
        public Uri MapProviderUri { get; set; } = new Uri("http://vanbassum.com:81/Maps/Maps.json");
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 1000;
        public string Nickname { get; set; } = "NoName";
    }

    [Serializable]
    public class MapProvider
    {
        public List<MapInfo> Maps = new List<MapInfo> { };
    }

    public class MapInfo
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
        public int MaxZoom { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
    */
}

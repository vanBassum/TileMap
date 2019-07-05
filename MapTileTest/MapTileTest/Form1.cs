using System;
using System.Drawing;
using System.Windows.Forms;
using TileMap;
using System.IO;

namespace MapTileTest
{
    public partial class Form1 : Form
    {
        Map map;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            map = new Map("Testmap\\Settings.json");
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

    

    
}

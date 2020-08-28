using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STDLib.Math;
using TileMapLib;
using System.IO;

namespace TileMapClient
{
    public partial class MapView : UserControl
    {
        public event EventHandler<V2D> MouseDownScaled;
        public event EventHandler<Graphics> OnDrawObjectsScaled;

        Map map;
        public V2D Center { get; set; } = new V2D(0, 0);
        public int Zoom { get; set; } = 0;

        private PictureBox picBox_Map = new PictureBox();
        private PictureBox picBox_Draw = new PictureBox();

        private V2D centerStartDrag;
        private Point mouseStartDrag;

        string mapDir;

        public MapView()
        {
            InitializeComponent();
        }

        public void SetMap(Map map)
        {
            this.map = map;
            mapDir = Path.Combine(Settings.MapsFolder, map.GUID.ToString());
        }

        private void MapView_Load(object sender, EventArgs e)
        {
            this.Controls.Add(picBox_Map);
            picBox_Map.Controls.Add(picBox_Draw);

            picBox_Map.Dock = DockStyle.Fill;
            picBox_Map.BackColor = Color.Transparent;
            picBox_Map.BringToFront();
            picBox_Map.Paint += PicBox_Map_Paint;

            picBox_Draw.Dock = DockStyle.Fill;
            picBox_Draw.BackColor = Color.Transparent;
            picBox_Draw.BringToFront();
            picBox_Draw.Paint += PicBox_Draw_Paint;

            picBox_Draw.MouseMove += picBox_MouseMove;
            picBox_Draw.MouseDown += picBox_MouseDown;
        }

        private void picBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (map != null)
            {
                centerStartDrag = Center;
                mouseStartDrag = e.Location;

                V2D pt = ((V2D)Center / map.TileSets[Zoom].Muliplier - ((V2D)this.Size / 2) + (V2D)e.Location) * map.TileSets[Zoom].Muliplier;
                MouseDownScaled?.Invoke(this, pt);
            }
        }

        private void picBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (map != null)
            {
                if (e.Button.HasFlag(MouseButtons.Left))
                {
                    Center = (V2D)centerStartDrag + ((V2D)mouseStartDrag - (V2D)e.Location) * map.TileSets[Zoom].Muliplier;
                    picBox_Draw.Refresh();
                    picBox_Map.Refresh();
                }
            }
        }

        private void PicBox_Draw_Paint(object sender, PaintEventArgs e)
        {
            if (map != null)
            {
                if (map.TileSets != null)
                {
                    if (map.TileSets.Count > Zoom)
                    {

                        V2D pt = ((V2D)this.Size / 2) - (V2D)Center / map.TileSets[Zoom].Muliplier;
                        e.Graphics.TranslateTransform((float)pt.X, (float)pt.Y);
                        e.Graphics.ScaleTransform((float)(1.0 / map.TileSets[Zoom].Muliplier), (float)(1.0 / map.TileSets[Zoom].Muliplier));
                        OnDrawObjectsScaled?.Invoke(this, e.Graphics);
                    }
                }
            }
        }

        private void PicBox_Map_Paint(object sender, PaintEventArgs e)
        {

            if (map != null)
            {
                if (map.TileSets != null)
                {
                    if (map.TileSets.Count > Zoom)
                    {
                        RectangleD viewPort = new RectangleD();
                        viewPort.Size = (V2D)this.Size;
                        viewPort.Position = (V2D)Center / map.TileSets[Zoom].Muliplier - ((V2D)viewPort.Size / 2);
                        foreach (Tile tile in map.TileSets[Zoom].Tiles)
                        {
                            RectangleD tileRect = new RectangleD(tile.Position, tile.Size);
                            if (RectangleD.Collides(viewPort, tileRect))
                            {
                                //Draw tile.
                                Image img = Image.FromFile(Path.Combine(mapDir, tile.ImageFile));
                                e.Graphics.DrawImage(img, (Point)(((V2D)tile.Position - (V2D)Center / map.TileSets[Zoom].Muliplier + ((V2D)viewPort.Size / 2))));
                                img.Dispose();
                            }
                        }
                    }
                }
            }
        }
    }
}

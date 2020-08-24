using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using FRMLib.TileImage;
using FRMLib.Scope;
using STDLib.JBVProtocol;
using STDLib.JBVProtocol.IO;
using STDLib.JBVProtocol.Connections;
using STDLib.Ethernet;
using FRMLib;
using FRMLib.Scope.Controls;

namespace MapTileTest
{
    public partial class Form1 : Form
    {
        Map map = new Map();

        TcpSocketClient socket = new TcpSocketClient();
        TCPConnection connection;
        Client client;

        Linear xLin = new Linear();
        Linear yLin = new Linear();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            socket.OnDisconnected += Socket_OnDisconnected;
            connection = new TCPConnection(socket);
            client = new Client();
            client.SetConnection(connection);

            /*
            MapCreator mc = new MapCreator();
            mc.ShowDialog();
            this.Close();
            */

            map.Load(@"C:\Users\Bas\Desktop\bRBCBwT\map.json");
            tileView1.Map = map;
            tileView1.Zoom = 0;
            tileView1.Center = new PointD(128, 128);
            tileView1.OnDrawObjectsScaled += TileView1_OnDrawObjectsScaled;
            tileView1.MouseDownScaled += TileView1_MouseDownScaled;
            timer1.Start();

            xLin.FromSamples(6804, 7128, 843, 871.5);
            yLin.FromSamples(3487, 3676, 313.5, 334);


            //xLin.FromSamples(7635, 6749, 887, 828);
            //yLin.FromSamples(4083, 3496, 350.5, 310.5);
        }

        

        private void TileView1_OnDrawObjectsScaled(object sender, Graphics e)
        {
            if(xLin != null && yLin != null && PlayerPos != null && PlayerRotation != null)
            {
                double x = xLin.Y(PlayerPos.X);
                double y = yLin.Y(PlayerPos.Y);

                e.DrawArrow(Pens.Red, new Point((int)x, (int)y), PlayerRotation.X, PlayerRotation.Y, 5);

            }

            //e.DrawCross(Pens.Blue, new Point(200, 200), 10);
            //e.DrawCross(Pens.Blue, new Point(500, 500), 10);
        }

        private void Socket_OnDisconnected(object sender, EventArgs e)
        {
            this.InvokeIfRequired(() => {
                this.Text = "Disconnected";
                button1.Enabled = true;
            });
        }

        private void tileView1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            if (await socket.ConnectAsync(Settings.HostAddress))
            {
                this.Text = "Connected";
                client.RequestLease();
            }
            else
            {
                this.Text = "Disconnected";
                button1.Enabled = true;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            tileView1.Zoom++;
            if (tileView1.Zoom >= map.TileSets.Count)
                tileView1.Zoom = 0;

            tileView1.Refresh();

        }


        enum State
        {
            NotAttached,
            Attached,
        }


        private void TileView1_MouseDownScaled(object sender, PointD e)
        {

            richTextBox1.AppendText($"xPos {e.X} => {PlayerPos.X}\r\n");
            richTextBox1.AppendText($"yPos {e.Y} => {PlayerPos.Y}\r\n");
        }

        MemoryManager wurm = new MemoryManager();
        State state = State.NotAttached;
        PointD PlayerPos { get; set; }
        PointD PlayerRotation { get; set; }

        private void timer1_Tick(object sender, EventArgs e)
        {

            switch (state)
            {
                case State.NotAttached:
                    wurm.attach("WurmLauncher64");
                    if (wurm.IsAttached())
                        state = State.Attached;
                    break;
                case State.Attached:
                    IntPtr moduleAddress = wurm.GetProcessModuleBase("OpenAL.dll");
                    IntPtr of1 = wurm.Read_Address(moduleAddress + 0xFCC28);
                    IntPtr of2 = wurm.Read_Address(of1 + 0x8);
                    float x = wurm.Read_Float(of2 + 0x0);
                    float y = wurm.Read_Float(of2 + 0x8);
                    float mx = wurm.Read_Float(of2 + 0x18);
                    float my = wurm.Read_Float(of2 + 0x20);
                    PlayerPos = new PointD(x, y);
                    PlayerRotation = new PointD(mx, my);
                    tileView1.Refresh();
                    break;



            }
        }
    }


    public class Linear
    { 
        //A * X + B
        public double A { get; set; } 
        public double B { get; set; }


        public void FromSamples(double x1, double x2, double y1, double y2)
        {
            A = (y2 - y1) / (x2 - x1);
            B = y1 - A * x1;
        }

        public double Y(double X)
        {
            return A * X + B;
        }

        public double X(double Y)
        {
            return (Y - B) / A;
        }
    }



}

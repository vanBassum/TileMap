using FRMLib;
using FRMLib.Scope.Controls;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TileMapLib;

namespace TileMapClient
{
    public partial class Form1 : Form
    {
        TileMapClient mapClient = new TileMapClient();
        MapInfo mapInfo;
        MapDownloader downloader = new MapDownloader();
        BindingList<Map> Maps = new BindingList<Map>();
        MemoryReader memoryReader;
        PlayerInfo localPlayer = new PlayerInfo();
        MapView mapview = new MapView();
        
        public Form1()
        {
            Settings.Load();
            InitializeComponent();
            mapClient.OnServerFound += MapClient_OnServerFound;
            mapClient.OnMapInfoRecieved += MapClient_OnMapInfoRecieved;
            downloader.OnProgressChanged += (a, b) => progressBar1.InvokeIfRequired(()=>progressBar1.Value = b);
            downloader.OnDone += (a, b) => this.InvokeIfRequired(()=> 
            { 
                Maps.Add(b);
                progressBar1.Value = 0;
                label2.Text = "";
                textBox1.Text = b.Name;
            });
            listBox2.DataSource = Maps;
            //listBox1.DataSource = mapClient.Servers;

            panel2.Controls.Add(mapview);
            mapview.Dock = DockStyle.Fill;
            mapview.OnDrawObjectsScaled += Mapview_OnDrawObjectsScaled;
            
        }

        private void Mapview_OnDrawObjectsScaled(object sender, Graphics e)
        {
            if(listBox2.SelectedItem is Map map)
            {
                //Draw the player.
                double x = map.ScaleX.Y(localPlayer.LocXPos);
                double y = map.ScaleY.Y(localPlayer.LocYPos);
                e.DrawArrow(Pens.Red, new Point((int)x, (int)y), localPlayer.DirXPos, localPlayer.DirYPos, 5);
            }

        }

        private void MemoryReader_OnPlayerInfoUpdate(object sender, PlayerInfo e)
        {
            this.InvokeIfRequired(()=>e.Username = textBox2.Text);
            localPlayer.Populate(e);
            this.InvokeIfRequired(()=>mapview.Refresh());
            mapClient.SendPlayerInfo(e);
        }

        private void MapClient_OnMapInfoRecieved(object sender, TileMapLib.MapInfo e)
        {
            mapInfo = e;
            textBox1.InvokeIfRequired(()=>textBox1.Text = mapInfo.GUID.ToString());
            //Check if map already exists otherwise download the map.

            Map serverMap = Maps.FirstOrDefault(m => m.GUID == mapInfo.GUID);
            if(serverMap != null)
            {
                this.InvokeIfRequired(() => textBox1.Text = serverMap.Name);
                this.InvokeIfRequired(() => listBox2.SelectedItem = serverMap);
            }
            else
            {
                label2.InvokeIfRequired(() => label2.Text = "Downloading");
                downloader.StartDownloadingMap(mapInfo);
            }
        }

        private void MapClient_OnServerFound(object sender, TileServer e)
        {
            listBox1.InvokeIfRequired(()=>listBox1.Items.Add(e));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach(string mapDir in Directory.GetDirectories(Settings.MapsFolder))
            {
                string mapJson = Path.Combine(mapDir, "map.json");
                Map m = new Map();
                m.Load(mapJson);
                Maps.Add(m); 
            }
            listBox2.SelectedIndex = -1;
            textBox2.Text = Settings.Username;
            /*
            if (listBox2.Items.Count > 0)
            { }
                listBox2.SelectedIndex = 0;
            */
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is TileServer serv)
                mapClient.ConnectToServer(serv);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox2.SelectedItem is Map map)
            {
                if (memoryReader != null)
                    memoryReader.Dispose();

                memoryReader = new MemoryReader(map);
                memoryReader.OnPlayerInfoUpdate += MemoryReader_OnPlayerInfoUpdate;
                mapview.SetMap(map);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            localPlayer.Username = textBox2.Text;
            Settings.Username = textBox2.Text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Save();
        }
    }


}

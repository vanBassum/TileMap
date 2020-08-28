using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using TileMapLib;

namespace TileMapClient
{
    public class MapDownloader
    {
        WebClient WebClient;
        public event EventHandler<int> OnProgressChanged;
        public event EventHandler<Map> OnDone;
        MapInfo mapinfo;

        string mapDir;
        string mapZip;
        string mapJson;

        public void StartDownloadingMap(MapInfo mapinfo)
        {
            this.mapinfo = mapinfo;
            WebClient = new WebClient();
            WebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            WebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

            mapDir = Path.Combine(Settings.MapsFolder, mapinfo.GUID.ToString());
            mapZip = Path.Combine(Settings.MapsFolder, mapinfo.GUID.ToString(), "map.tilemap");
            mapJson = Path.Combine(Settings.MapsFolder, mapinfo.GUID.ToString(), "map.json");

            Directory.CreateDirectory(mapDir);
            WebClient.DownloadFileAsync(new Uri(mapinfo.DownloadLocation), mapZip);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressChanged?.Invoke(this, e.ProgressPercentage);
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //unzip
            ZipFile.ExtractToDirectory(mapZip, mapDir);
            File.Delete(mapZip);
            Map map = new Map();
            map.Load(mapJson);
            OnDone?.Invoke(this, map);
        }

    }

}

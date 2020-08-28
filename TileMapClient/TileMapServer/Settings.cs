using STDLib.Saveable;
using System;
using TileMapLib;

namespace TileMapServer
{
    public class Settings : BaseSettings<Settings>
    {
        public static string ConnectionServer { get { return GetPar<string>("127.0.0.1:1000"); } set { SetPar(value); } }
        public static string Servername { get { return GetPar<string>("SomeServer"); } set { SetPar(value); } }
        public static MapInfo MapInfo { get { return GetPar<MapInfo>(new MapInfo { DownloadLocation = "http://vanBassum.com:81/Maps/Cadence.tilemap", GUID = Guid.Parse("f8225f37-5589-40f9-bc24-306e907a08d2") }); } set { SetPar(value); } }
    }


}

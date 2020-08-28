using STDLib.Saveable;
using System.IO;

namespace TileMapClient
{
    public class Settings : BaseSettings<Settings>
    {
        public static string ConnectionServer { get { return GetPar<string>("127.0.0.1:1000"); } set { SetPar(value); } }
        public static string Username { get { return GetPar<string>("Noname"); } set { SetPar(value); } }
        public static string MapsFolder { get { return GetPar<string>(Path.Combine(DataFolder, "Maps")); } set { SetPar(value); } }
        //public static bool ShowConsole { get { return GetPar<bool>(true); } set { SetPar(value); } }
    }


}

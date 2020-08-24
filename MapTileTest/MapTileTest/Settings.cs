using STDLib.Saveable;

namespace MapTileTest
{
    public sealed class Settings : BaseSettings<Settings>
    {
        public static string HostAddress { get { return GetPar<string>("127.0.0.1:1000"); } set { SetPar(value); } }
        
    }
}

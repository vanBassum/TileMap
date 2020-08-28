using STDLib.Saveable;
using System.IO;

namespace TileMapCreator
{
    public class Settings : BaseSettings<Settings>
    {
        public static string TempFolder
        {
            get { return GetPar<string>(Path.Combine(DataFolder, "Temp")); }
            set { SetPar(value); }
        }
    }
}

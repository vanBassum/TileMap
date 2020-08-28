using STDLib.Math;
using STDLib.Saveable;
using System;
using System.ComponentModel;

namespace TileMapLib
{
    public class Map : Saveable
    {
        /// <summary>
        /// Each map is indentified by an unique guid.
        /// This way the server and client know they have the same map.
        /// </summary>
        public Guid GUID { get; set; }
        public string Name { get; set; } = "Noname";
        public BindingList<TileSet> TileSets { get; set; } = new BindingList<TileSet>();

        public string ProcessName { get; set; } = "WurmLauncher64";
        public string AddressLocX { get; set; } = "OpenAL.dll]FCC28]8]0]";
        public string AddressLocY { get; set; } = "OpenAL.dll]FCC28]8]8]";
        public string AddressDirX { get; set; } = "OpenAL.dll]FCC28]8]18]";
        public string AddressDirY { get; set; } = "OpenAL.dll]FCC28]8]20]";

        public Linear ScaleX { get; set; } = Linear.FromSamples(6804, 7128, 843, 871.5);
        public Linear ScaleY { get; set; } = Linear.FromSamples(3487, 3676, 313.5, 334);

        public override string ToString()
        {
            return Name;
        }
    }


}

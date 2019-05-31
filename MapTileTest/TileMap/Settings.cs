using Datasave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMap
{
    [Serializable]
    class MapSettings : SettingsExtention
    {
        public string Mapname { get => GetPar<string>(); set => SetPar(value); }
        public int Zoomlevels { get => GetPar<int>(); set => SetPar(value); }
        public Size Tilesize { get => GetPar<Size>(); set => SetPar(value); }
        public List<KeyValuePair<Point, Coordinate>> CalibrationPoints { get => GetPar<List<KeyValuePair<Point, Coordinate>>>(); set => SetPar(value); }


        public MapSettings() : base(new Serializer_JSON())
        {
            Mapname = "TestMap";
            Zoomlevels = 6;
            Tilesize = new Size(256, 256);



            CalibrationPoints = new List<KeyValuePair<Point, Coordinate>>{
            new KeyValuePair<Point, Coordinate>(new Point(1,1),new Coordinate(1,1)),
            new KeyValuePair<Point, Coordinate>(new Point(2,2),new Coordinate(2,2))};
        }

    }
}

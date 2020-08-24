using STDLib.Saveable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMap
{
    public class MapSettings : SaveableSettings
    {
        public string Mapname { get; set; }
        public int Zoomlevels { get; set; }
        public Size Tilesize { get; set; } = new Size(256, 256);
        public List<KeyValuePair<Point, Coordinate>> CalibrationPoints { get; set; } = new List<KeyValuePair<Point, Coordinate>>();

        /*
        public MapSettings() : base(new Serializer_JSON())
        {
            Mapname = "TestMap";
            Zoomlevels = 6;
            Tilesize = new Size(256, 256);



            CalibrationPoints = new List<KeyValuePair<Point, Coordinate>>{
            new KeyValuePair<Point, Coordinate>(new Point(1,1),new Coordinate(1,1)),
            new KeyValuePair<Point, Coordinate>(new Point(2,2),new Coordinate(2,2))};
        }
        */

    }
}

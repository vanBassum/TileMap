using System;

namespace TileMap
{
    [Serializable]
    public class Coordinate
    {
        public double X { get; set; }
        public double Y { get; set; }


        public Coordinate()
        {
        }

        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Coordinate operator +(Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1.X+c2.X, c1.Y + c2.Y);
        }

        public static Coordinate operator -(Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1.X - c2.X, c1.Y - c2.Y);
        }
    }
        
    
}

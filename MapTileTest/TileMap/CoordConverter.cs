using System;
using System.Collections.Generic;
using System.Drawing;

namespace TileMap
{

    public class CoordConverter
    {
        private double[] PolyX = new double[] { 0, 1 };
        private double[] PolyY = new double[] { 0, 1 };

        public void Calibrate(List<KeyValuePair<Point, Coordinate>> calPoints)
        {
            //Do linear regression and update polyx and y
            List<XYPoint> xPts = new List<XYPoint> { };
            List<XYPoint> yPts = new List<XYPoint> { };

            foreach (KeyValuePair<Point, Coordinate> pt in calPoints)
            {
                xPts.Add(new XYPoint(pt.Key.X, pt.Value.X));
                yPts.Add(new XYPoint(pt.Key.Y, pt.Value.Y));
            }

            PolyX = LinearRegression(xPts.ToArray());
            PolyY = LinearRegression(yPts.ToArray());

        }


        private double[] LinearRegression(XYPoint[] points)
        {
            double[] coofs = new double[2];

            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumXX = 0;
            

            foreach(XYPoint pt in points)
            {
                sumX += pt.X;
                sumY += pt.Y;
                sumXX += pt.X * pt.X;
                sumXY += pt.X * pt.Y;
            }

            coofs[1] = (sumXY - (sumX * sumY)) / (sumXX - sumX * sumX);
            coofs[0] = (sumY - coofs[1] * sumX) / points.Length;

            return coofs;
        }

        struct XYPoint
        {
            public double X { get; set; }
            public double Y { get; set; }

            public XYPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }


        public Coordinate Convert(Point pt, int zoomLevel)
        {
            int mp = (int)Math.Pow(2, zoomLevel);
            pt.X /= mp;
            pt.Y /= mp;

            return new Coordinate((pt.X - PolyX[0])/PolyX[1], (pt.Y - PolyY[0]) / PolyY[1]);
        }

        public Point Convert(Coordinate coord, int zoomLevel)
        {
            Point pt = new Point((int)(coord.X * PolyX[1] + PolyX[0]), (int)(coord.Y * PolyY[1] + PolyY[0]));
            int mp = (int)Math.Pow(2, zoomLevel);
            pt.X *= mp;
            pt.Y *= mp;

            return pt;
        }

    }
        
    
}

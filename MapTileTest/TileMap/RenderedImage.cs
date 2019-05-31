using System;
using System.Drawing;

namespace TileMap
{
    class RenderedImage : ICloneable
    {
        public Image Image { get; set; }
        public Coordinate TopLeft { get; set; }
        public void Dispose()
        {
            Image.Dispose();
        }

        public object Clone()
        {
            RenderedImage ri = new RenderedImage();
            if (Image != null)
                ri.Image = Image.Clone() as Image;
            ri.TopLeft = TopLeft;
            return ri;
        }
    }
        
    
}

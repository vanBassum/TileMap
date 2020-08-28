using STDLib.Math;

namespace TileMapLib
{
    public class Tile
    {
        public string ImageFile { get; set; }
        public V2D Position { get; set; }
        public V2D Size { get; set; }

        public Tile()
        {

        }

        public Tile(string image, V2D pos, V2D size)
        {
            ImageFile = image;
            Position = pos;
            Size = size;
        }
    }
}


namespace ECS.Components.PVZ
{
    public struct Tile
    {
        public Entity? Entity;
    }

    public struct GridSize
    {
        public int Rows;
        public int Columns;
        public int TileSize; // Squares, so just provide one side

    }

    public struct TileGrid
    {
        public Tile[,] Tiles;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.PvZ.Components
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

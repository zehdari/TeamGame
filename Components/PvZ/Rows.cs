
namespace ECS.Components.PVZ
{
    public struct GridInfo
    {
        /* 2D array of nullable entities. null == no entity in that square */
        public Entity?[][] RowInfo;
        public int TileSize;
        public int NumRows;
        public int NumColumns;
    }
}


namespace ECS.Components.PVZ
{
    public struct GridInfo
    {
        /* Use index for row, list of entities for what plants are in that row */
        public Entity?[][] RowInfo;
        public int TileSize;
        public int NumRows;
        public int NumColumns;
    }
}

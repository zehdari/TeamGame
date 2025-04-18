using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.PvZ.Components
{
    public struct GridInfo
    {
        /* Use index for row, list of entities for what plants are in that row */
        public Entity?[][] RowInfo;
        public int TileSize;
    }
}

namespace ECS.Core.Utilities;

public class SpatialGrid
{
    private readonly int cellSize;
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly Dictionary<(int, int), HashSet<Entity>> cells;

    public int CellSize => cellSize;
    public int Width => gridWidth;
    public int Height => gridHeight;

    // Add method to get occupied cells for visualization
    public IEnumerable<(int x, int y)> GetOccupiedCells()
    {
        return cells.Keys;
    }

    // Add method to get adjacent cells for an entity
    public IEnumerable<(int x, int y)> GetAdjacentCells(Entity entity, Rectangle bounds)
    {
        var baseCells = GetOverlappingCells(bounds);
        var adjacentCells = new HashSet<(int, int)>();

        foreach (var (x, y) in baseCells)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var neighborCoord = (x + dx, y + dy);
                    if (neighborCoord.Item1 >= 0 && neighborCoord.Item1 < gridWidth &&
                        neighborCoord.Item2 >= 0 && neighborCoord.Item2 < gridHeight)
                    {
                        adjacentCells.Add(neighborCoord);
                    }
                }
            }
        }

        return adjacentCells;
    }

    // Add method to get directly occupied cells for an entity
    public IEnumerable<(int x, int y)> GetEntityCells(Entity entity, Rectangle bounds)
    {
        return GetOverlappingCells(bounds);
    }


    public SpatialGrid(Point windowSize, int cellSize = 100)
    {
        this.cellSize = cellSize;
        this.gridWidth = (windowSize.X / cellSize) + 1;
        this.gridHeight = (windowSize.Y / cellSize) + 1;
        this.cells = new Dictionary<(int, int), HashSet<Entity>>();
    }

    public void Clear()
    {
        cells.Clear();
    }

    public void InsertEntity(Entity entity, Rectangle bounds)
    {
        var cellCoords = GetOverlappingCells(bounds);
        foreach (var coord in cellCoords)
        {
            if (!cells.ContainsKey(coord))
            {
                cells[coord] = new HashSet<Entity>();
            }
            cells[coord].Add(entity);
        }
    }

    public HashSet<Entity> GetPotentialCollisions(Entity entity, Rectangle bounds)
    {
        var potentialCollisions = new HashSet<Entity>();
        var cellCoords = GetOverlappingCells(bounds);
        var checkedCells = new HashSet<(int, int)>();

        foreach (var (x, y) in cellCoords)
        {
            // Check the current cell and all adjacent cells
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var neighborCoord = (x + dx, y + dy);
                    
                    // Skip if outside grid bounds
                    if (neighborCoord.Item1 < 0 || neighborCoord.Item1 >= gridWidth ||
                        neighborCoord.Item2 < 0 || neighborCoord.Item2 >= gridHeight)
                        continue;

                    // Skip if we've already checked this cell
                    if (!checkedCells.Add(neighborCoord))
                        continue;

                    // Add any entities from this cell
                    if (cells.TryGetValue(neighborCoord, out var cellEntities))
                    {
                        foreach (var other in cellEntities)
                        {
                            if (!other.Equals(entity))
                            {
                                potentialCollisions.Add(other);
                            }
                        }
                    }
                }
            }
        }

        return potentialCollisions;
    }

    private List<(int, int)> GetOverlappingCells(Rectangle bounds)
    {
        var overlappingCells = new List<(int, int)>();
        
        // Convert bounds to cell coordinates (using integer division)
        int startX = Math.Max(0, bounds.Left / cellSize);
        int startY = Math.Max(0, bounds.Top / cellSize);
        int endX = Math.Min(gridWidth - 1, bounds.Right / cellSize);
        int endY = Math.Min(gridHeight - 1, bounds.Bottom / cellSize);

        // Get all cells this object overlaps
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                overlappingCells.Add((x, y));
            }
        }

        return overlappingCells;
    }
}
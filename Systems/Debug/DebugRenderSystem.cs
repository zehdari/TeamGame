using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Collision;
using ECS.Components.Animation;

namespace ECS.Systems.Debug;
   
public class DebugRenderSystem : SystemBase
{
    private readonly SpriteBatch spriteBatch;
    private Texture2D pixel;
    private SpriteFont debugFont;
    private int frameRate = 0;
    private int frameCounter = 0;
    private TimeSpan elapsedTime = TimeSpan.Zero;
    private SpatialGrid spatialGrid;

    // Debug toggle flags
    private bool showDebug = false;
    private bool showHitboxes = false;
    private bool showEntityIDs = false;
    private bool showPlayerState = false;
    private bool showSpatialGrid = false;

    // List to store collision events for drawing their normals.
    private List<CollisionEvent> debugCollisionEvents = new List<CollisionEvent>();

    public override bool Pausible => false;

    public DebugRenderSystem(GameAssets assets, GraphicsManager graphicsManager)
    {
        this.debugFont = assets.GetFont("DebugFont");
        this.spriteBatch = graphicsManager.spriteBatch;
        this.spatialGrid = graphicsManager.spatialGrid;
        
        // Create the pixel texture during initialization
        pixel = new Texture2D(graphicsManager.graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAction);
        Subscribe<CollisionEvent>(HandleCollisionEvent);
    }

    private void HandleAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;
        if (!actionEvent.IsStarted) return;

        // I know... but it's debug so I was lazy
        switch (actionEvent.ActionName.ToLower())
        {
            case "toggle_debug":
                showDebug = !showDebug;
                Console.WriteLine($"Debug rendering: {showDebug}");
                break;
            case "toggle_hitboxes":
                showHitboxes = !showHitboxes;
                Console.WriteLine($"Hitbox rendering: {showHitboxes}");
                break;
            case "toggle_entity_ids":
                showEntityIDs = !showEntityIDs;
                Console.WriteLine($"Entity ID rendering: {showEntityIDs}");
                break;
            case "toggle_player_state":
                showPlayerState = !showPlayerState;
                Console.WriteLine($"Player state rendering: {showPlayerState}");
                break;
            case "toggle_spatial_grid":
                showSpatialGrid = !showSpatialGrid;
                Console.WriteLine($"Spatial grid rendering: {showSpatialGrid}");
                break;
        }
    }

    // Subscribe to collision events so we can later draw the contact normals.
    private void HandleCollisionEvent(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        // Only record Begin and Stay events
        if (collisionEvent.EventType != CollisionEventType.End)
        {
            debugCollisionEvents.Add(collisionEvent);
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        if (!showDebug) return;

        // Increment frame counter for FPS calculation.
        frameCounter++;
        CalculateFPS(gameTime);

        // Draw Acceleration Vectors (in Red)
        DrawAccelerationVectors(spriteBatch);

        // Draw Velocity Vectors (in Green)
        DrawVelocityVectors(spriteBatch);

        // Draw hitboxes, polygon normals and contact normals from collision events
        if (showHitboxes)
        {
            DrawHitboxes(spriteBatch);
            DrawCollisionContactNormals(spriteBatch);
        }

        // Draw spatial grid if enabled
        if (showSpatialGrid)
        {
            DrawSpatialGrid(spriteBatch);
        }

        // Draw the FPS Counter
        DrawFPSCounter(spriteBatch);

        // Draw entity IDs if enabled
        if (showEntityIDs)
        {
            DrawEntityIDs(spriteBatch);
        }

        // Draw player state text if enabled
        if (showPlayerState)
        {
            DrawPlayerStateText(spriteBatch);
        }

        // Clear stored collision events after drawing so they don't accumulate.
        debugCollisionEvents.Clear();
    }

    private void CalculateFPS(GameTime gameTime)
    {
        elapsedTime += gameTime.ElapsedGameTime;

        if (elapsedTime > TimeSpan.FromSeconds(1))
        {
            elapsedTime -= TimeSpan.FromSeconds(1);
            frameRate = frameCounter;
            frameCounter = 0;
        }
    }

    private void DrawFPSCounter(SpriteBatch spriteBatch)
    {
        string fpsText = $"FPS: {frameRate}";
        Vector2 pos = new Vector2(10, 10);
        DrawOutlinedText(spriteBatch, fpsText, pos);
    }

    private void DrawAccelerationVectors(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || !HasComponents<Acceleration>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var acceleration = ref GetComponent<Acceleration>(entity);

            if (acceleration.Value == Vector2.Zero)
                continue;

            DrawVector(spriteBatch, position.Value, acceleration.Value, Color.Red, 0.1f);
        }
    }

    private void DrawVelocityVectors(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || !HasComponents<Velocity>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            if (velocity.Value == Vector2.Zero)
                continue;

            DrawVector(spriteBatch, position.Value, velocity.Value, Color.Green, 0.1f);
        }
    }

    private void DrawSpatialGrid(SpriteBatch spriteBatch)
    {
        // Draw grid lines
        int cellSize = spatialGrid.CellSize;
        for (int x = 0; x <= spatialGrid.Width; x++)
        {
            Vector2 start = new Vector2(x * cellSize, 0);
            Vector2 end = new Vector2(x * cellSize, spatialGrid.Height * cellSize);
            DrawLine(spriteBatch, start, end, Color.Gray * 0.5f, 1f);
        }
        
        for (int y = 0; y <= spatialGrid.Height; y++)
        {
            Vector2 start = new Vector2(0, y * cellSize);
            Vector2 end = new Vector2(spatialGrid.Width * cellSize, y * cellSize);
            DrawLine(spriteBatch, start, end, Color.Gray * 0.5f, 1f);
        }
        
        // Dictionary to keep track of which cells we've drawn outlines for
        var drawnCells = new Dictionary<(int, int), Color>();
        
        // Get all entities with collision and velocity 
        foreach (var entity in World.GetEntities())
        {
            // Only show entities with velocity components
            if (HasComponents<CollisionBody>(entity) && 
                HasComponents<Position>(entity) && 
                HasComponents<Velocity>(entity))
            {
                // Get collision bounds of the entity
                ref var body = ref GetComponent<CollisionBody>(entity);
                ref var pos = ref GetComponent<Position>(entity);
                ref var velocity = ref GetComponent<Velocity>(entity);
                
                // Default color for velocity entities
                Color occupiedColor = Color.Blue;
                Color checkColor = Color.Yellow;
                
                // Find the bounding rectangle
                Rectangle bounds = GetEntityBounds(entity, body, pos);
                
                // Get cells directly occupied by the entity
                var occupiedCells = spatialGrid.GetEntityCells(entity, bounds).ToList();
                
                // Draw occupied cells
                foreach (var (x, y) in occupiedCells)
                {
                    // Always override with occupied color (it's more important)
                    drawnCells[(x, y)] = occupiedColor;
                }
                
                // Get potential collision cells (adjacent cells)
                var checkCells = spatialGrid.GetAdjacentCells(entity, bounds).ToList();
                
                // Track cells that would be checked for collisions
                foreach (var (x, y) in checkCells)
                {
                    // Only add to drawn cells if not already occupied by this or another entity
                    if (!occupiedCells.Contains((x, y)) && !drawnCells.ContainsKey((x, y)))
                    {
                        drawnCells[(x, y)] = checkColor;
                    }
                }
            }
        }
        
        // Draw all occupied cells first (but only color the background for occupied cells with entities)
        foreach (var (x, y) in spatialGrid.GetOccupiedCells())
        {
            Vector2 cellPos = new Vector2(x * cellSize, y * cellSize);
            
            // Fill with a semi-transparent color
            spriteBatch.Draw(
                pixel,
                new Rectangle((int)cellPos.X, (int)cellPos.Y, cellSize, cellSize),
                null,
                new Color(255, 0, 0, 50)
            );
        }
        
        // Now draw all cell outlines based on our tracking
        foreach (var ((x, y), color) in drawnCells)
        {
            Vector2 cellPos = new Vector2(x * cellSize, y * cellSize);
            DrawCellOutline(spriteBatch, cellPos, cellSize, color);
        }
    }
    
    private void DrawCellOutline(SpriteBatch spriteBatch, Vector2 position, int size, Color color)
    {
        DrawLine(spriteBatch, position, position + new Vector2(size, 0), color, 2f);
        DrawLine(spriteBatch, position, position + new Vector2(0, size), color, 2f);
        DrawLine(spriteBatch, position + new Vector2(size, 0), position + new Vector2(size, size), color, 2f);
        DrawLine(spriteBatch, position + new Vector2(0, size), position + new Vector2(size, size), color, 2f);
    }
    
    private Entity GetFocusedEntity(World world)
    {
        // Option 1: Get the player entity (if there's a player)
        foreach (var entity in world.GetEntities())
        {
            if (HasComponents<PlayerStateComponent>(entity))
            {
                return entity;
            }
        }
        
        // Default: return the first entity with collision
        foreach (var entity in world.GetEntities())
        {
            if (HasComponents<CollisionBody>(entity))
            {
                return entity;
            }
        }
        
        return default;
    }
    
    private Rectangle GetEntityBounds(Entity entity, CollisionBody body, Position pos)
    {
        // Get the scale if available
        Scale scale = HasComponents<Scale>(entity) ? GetComponent<Scale>(entity) : default;
        
        // Calculate bounding box for all polygons
        Vector2 min = new Vector2(float.MaxValue);
        Vector2 max = new Vector2(float.MinValue);
        
        foreach (var polygon in body.Polygons)
        {
            var vertices = PolygonTools.GetTransformedVertices(entity, polygon, pos, scale);
            
            foreach (var vertex in vertices)
            {
                min = Vector2.Min(min, vertex);
                max = Vector2.Max(max, vertex);
            }
        }
        
        return new Rectangle(
            (int)min.X,
            (int)min.Y,
            (int)(max.X - min.X),
            (int)(max.Y - min.Y)
        );
    }

    private void DrawVector(SpriteBatch spriteBatch, Vector2 origin, Vector2 vectorValue, Color color, float scaleFactor)
    {
        Vector2 endPoint = origin + vectorValue * scaleFactor;
        DrawLine(spriteBatch, origin, endPoint, color, 2f);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            pixel,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(edge.Length(), thickness),
            SpriteEffects.None,
            0
        );
    }

    // Draw the player's current state as text.
    private void DrawPlayerStateText(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<PlayerStateComponent>(entity) || !HasComponents<Position>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var playerState = ref GetComponent<PlayerStateComponent>(entity);

            string stateText = playerState.CurrentState.ToString();
            Vector2 textSize = debugFont.MeasureString(stateText);
            // Center text above the player's head
            Vector2 textPosition = position.Value - new Vector2(textSize.X / 2, 40 + textSize.Y);
            DrawOutlinedText(spriteBatch, stateText, textPosition);
        }
    }

    // Draw hitboxes for entities with a CollisionBody component.
    private void DrawHitboxes(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<CollisionBody>(entity) || !HasComponents<Position>(entity))
                continue;

            ref var body = ref GetComponent<CollisionBody>(entity);
            ref var pos = ref GetComponent<Position>(entity);

            foreach (var polygon in body.Polygons)
            {
                // Compute transformed vertices
                Scale scale = HasComponents<Scale>(entity) ? GetComponent<Scale>(entity) : default;
                var vertices = PolygonTools.GetTransformedVertices(entity, polygon, pos, scale);

                for (int i = 0; i < vertices.Length; i++)
                {
                    var start = vertices[i];
                    var end = vertices[(i + 1) % vertices.Length];
                    DrawLine(spriteBatch, start, end, GetColorForLayer(polygon.Layer), 1f);
                }

                DrawPolygonNormals(spriteBatch, vertices);
            }
        }
    }

    // Draw normals for each edge of a polygon.
    private void DrawPolygonNormals(SpriteBatch spriteBatch, Vector2[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            var start = vertices[i];
            var end = vertices[(i + 1) % vertices.Length];
            var mid = (start + end) / 2;
            var edge = end - start;
            var normal = new Vector2(-edge.Y, edge.X);
            if (normal != Vector2.Zero)
                normal.Normalize();

            DrawLine(spriteBatch, mid, mid + normal * 10, Color.Yellow, 1f);
        }
    }

    // Draw collision contact normals from collision events.
    private void DrawCollisionContactNormals(SpriteBatch spriteBatch)
    {
        foreach (var collisionEvent in debugCollisionEvents)
        {
            var contact = collisionEvent.Contact;
            Vector2 start = contact.Point;
            Vector2 end = contact.Point + contact.Normal * 20f;
            DrawLine(spriteBatch, start, end, Color.Orange, 2f);
        }
    }

    private Color GetColorForLayer(CollisionLayer layer)
    {
        return layer switch
        {
            CollisionLayer.World => Color.White,
            CollisionLayer.Physics => Color.Yellow,
            CollisionLayer.Hitbox => Color.Red,
            CollisionLayer.Hurtbox => Color.Blue,
            CollisionLayer.Trigger => Color.Green,
            _ => Color.Gray
        };
    }

    // Draws a rectangle outline using lines.
    private void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color)
    {
        Vector2 topLeft = position;
        Vector2 topRight = position + new Vector2(size.X, 0);
        Vector2 bottomLeft = position + new Vector2(0, size.Y);
        Vector2 bottomRight = position + size;

        DrawLine(spriteBatch, topLeft, topRight, color, 1f);
        DrawLine(spriteBatch, topLeft, bottomLeft, color, 1f);
        DrawLine(spriteBatch, topRight, bottomRight, color, 1f);
        DrawLine(spriteBatch, bottomLeft, bottomRight, color, 1f);
    }

    // Draws entity IDs centered on the entity's position.
    private void DrawEntityIDs(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            string idText = entity.Id.ToString();
            Vector2 textSize = debugFont.MeasureString(idText);
            Vector2 textPosition = position.Value - textSize / 2;
            DrawOutlinedText(spriteBatch, idText, textPosition);
        }
    }

    // Helper method to draw outlined text.
    private void DrawOutlinedText(SpriteBatch spriteBatch, string text, Vector2 position)
    {
        float outlineOffset = 2f;
        // Draw the black outline in 8 directions.
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, 0), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, 0), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, -outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, -outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, -outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, outlineOffset), Color.Black);

        // Draw the white text on top.
        spriteBatch.DrawString(debugFont, text, position, Color.White);
    }
}
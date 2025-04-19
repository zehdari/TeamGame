using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Collision;
using ECS.Components.Animation;
using ECS.Core.Utilities;

namespace ECS.Systems.Debug;
   
public class DebugRenderSystem : SystemBase
{
    private readonly GraphicsManager graphicsManager;
    private readonly SpriteBatch spriteBatch;
    private Texture2D pixel;
    private SpriteFont debugFont;
    private int frameRate = 0;
    private int frameCounter = 0;
    private TimeSpan elapsedTime = TimeSpan.Zero;
    private float layerDepth;
     private float layerDepthTop;

    // Debug toggle flags
    private bool showFPS = false;
    private bool showHitboxes = false;
    private bool showEntityIDs = false;
    private bool showPlayerState = false;
    private bool showMovementVectors = false;
    private bool showMouseCoordinates = false;
    private bool polygonCreationMode = false;
    
    // List to store collision events for drawing their normals.
    private List<CollisionEvent> debugCollisionEvents = new List<CollisionEvent>();
    
    // Polygon creation tool
    private List<Vector2> polygonPoints = new List<Vector2>();
    private bool previousLeftButtonPressed = false;
    private bool previousRightButtonPressed = false;

    public override bool Pausible => false;
    public override bool UseScaledGameTime => false;

    public DebugRenderSystem(GameAssets assets, GraphicsManager graphicsManager)
    {
        this.debugFont = assets.GetFont("DebugFont");
        this.graphicsManager = graphicsManager;
        this.spriteBatch = graphicsManager.spriteBatch;
        this.layerDepth = graphicsManager.GetLayerDepth(DrawLayer.Debug);
        this.layerDepthTop = graphicsManager.GetLayerDepth(DrawLayer.DebugText);
        
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
            case "toggle_fps":
                showFPS = !showFPS;
                Console.WriteLine($"FPS rendering: {showFPS}");
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
            case "toggle_movement_vectors":
                showMovementVectors = !showMovementVectors;
                Console.WriteLine($"Movement vector rendering: {showMovementVectors}");
                break;
            case "toggle_mouse_coordinates":
                showMouseCoordinates = !showMouseCoordinates;
                Console.WriteLine($"Mouse coordinate rendering: {showMouseCoordinates}");
                break;
            case "toggle_polygon_creation":
                polygonCreationMode = !polygonCreationMode;
                if (!polygonCreationMode) {
                    // Output the polygon points when exiting creation mode
                    OutputPolygonPoints();
                    polygonPoints.Clear();
                }
                Console.WriteLine($"Polygon creation mode: {polygonCreationMode}");
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
        // Get camera information
        Matrix cameraMatrix = graphicsManager.cameraManager.GetTransformMatrix();
        Matrix invertedCameraMatrix = Matrix.Invert(cameraMatrix);
        float cameraZoom = graphicsManager.cameraManager.GetZoom();

        // First draw all world-space debug elements (affected by camera)
        if (showMovementVectors)
        {
            // Draw Acceleration Vectors (in Red)
            DrawAccelerationVectors(spriteBatch);

            // Draw Velocity Vectors (in Green)
            DrawVelocityVectors(spriteBatch);
        }

        // Draw hitboxes, polygon normals and contact normals from collision events
        if (showHitboxes)
        {
            DrawHitboxes(spriteBatch);
            DrawCollisionContactNormals(spriteBatch);
        }

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

        // Handle polygon creation if enabled
        if (polygonCreationMode)
        {
            UpdatePolygonCreation(invertedCameraMatrix);
            DrawPolygonCreationPreview(spriteBatch);
        }

        // Now draw UI debug elements with counter-scaling so they appear at a fixed size
        if (showFPS)
        {
            // Increment frame counter for FPS calculation.
            frameCounter++;
            CalculateFPS(gameTime);

            // Draw the FPS Counter using screen-space position but with counter-scaling
            Vector2 screenPos = new Vector2(10, 10);
            // Convert to world space for drawing
            Vector2 worldPos = Vector2.Transform(screenPos, invertedCameraMatrix);
            DrawFPSCounter(spriteBatch, worldPos, cameraZoom);
        }

        if (showMouseCoordinates)
        {
            // Get current mouse state
            MouseState mouseState = Mouse.GetState();
            Vector2 mouseScreen = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorld = Vector2.Transform(mouseScreen, invertedCameraMatrix);
            
            // Calculate position for the text - in screen space
            Vector2 screenPos = showFPS ? new Vector2(10, 40) : new Vector2(10, 10);
            // Convert to world space for drawing
            Vector2 worldPos = Vector2.Transform(screenPos, invertedCameraMatrix);
            
            DrawMouseCoordinates(spriteBatch, worldPos, mouseScreen, mouseWorld, cameraZoom);
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

    private void DrawFPSCounter(SpriteBatch spriteBatch, Vector2 position, float cameraZoom)
    {
        string fpsText = $"FPS: {frameRate}";
        DrawOutlinedTextFixedSize(spriteBatch, fpsText, position, cameraZoom);
    }

    private void DrawMouseCoordinates(SpriteBatch spriteBatch, Vector2 position, Vector2 mouseScreen, Vector2 mouseWorld, float cameraZoom)
    {
        // Create text showing coordinates
        string mouseTextScreen = $"Mouse Screen: X={mouseScreen.X}, Y={mouseScreen.Y}";
        string mouseTextWorld = $"Mouse World: X={mouseWorld.X:F0}, Y={mouseWorld.Y:F0}";
        
        // Draw the text with fixed size
        DrawOutlinedTextFixedSize(spriteBatch, mouseTextScreen, position, cameraZoom);
        
        // Calculate offset for the second line - scaled by camera zoom
        float lineHeight = 30f / cameraZoom; // Scale the line height
        Vector2 secondLinePos = position + new Vector2(0, lineHeight);
        DrawOutlinedTextFixedSize(spriteBatch, mouseTextWorld, secondLinePos, cameraZoom);
        
        // If in polygon creation mode, show additional info
        if (polygonCreationMode)
        {
            string pointCountText = $"Points: {polygonPoints.Count} (Left click to add, Right click to remove last)";
            Vector2 thirdLinePos = secondLinePos + new Vector2(0, lineHeight);
            DrawOutlinedTextFixedSize(spriteBatch, pointCountText, thirdLinePos, cameraZoom);
        }
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
            layerDepth
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

    // Draw hitboxes for entities with a CollisionBody component
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

    // Draw normals for each edge of a polygon
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

    // Draw collision contact normals from collision events
    private void DrawCollisionContactNormals(SpriteBatch spriteBatch)
    {
        foreach (var collisionEvent in debugCollisionEvents)
        {
            var contact = collisionEvent.Contact;

            if (HasComponents<CollisionBody>(contact.EntityA) && HasComponents<Position>(contact.EntityA))
            {
                ref var body = ref GetComponent<CollisionBody>(contact.EntityA);
                ref var pos = ref GetComponent<Position>(contact.EntityA);
                Rectangle bounds = GetEntityBounds(contact.EntityA, body, pos);
                Vector2 center = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);

                Vector2 start = center;
                Vector2 end = center + contact.Normal * 20f;
                DrawLine(spriteBatch, start, end, Color.Orange, 2f);
            }
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

    // Draws a rectangle outline using lines
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

    // Helper method to draw outlined text that scales with camera
    private void DrawOutlinedText(SpriteBatch spriteBatch, string text, Vector2 position)
    {
        float outlineOffset = 2f;
        // Draw the black outline in 8 directions
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, 0), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, 0), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, -outlineOffset), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, outlineOffset), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, -outlineOffset), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, -outlineOffset), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, outlineOffset), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, outlineOffset), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);

        // Draw the white text on top
        spriteBatch.DrawString(debugFont, text, position, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepthTop);
    }
    
    // Helper method to draw outlined text that maintains fixed size regardless of camera zoom
    private void DrawOutlinedTextFixedSize(SpriteBatch spriteBatch, string text, Vector2 position, float cameraZoom)
    {
        // Apply inverse scale to counter the camera zoom
        float scale = 1.0f / cameraZoom;
        float outlineOffset = 2f / cameraZoom; // Scale outline offset too
        
        // Draw the black outline in 8 directions with counter-scaling
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, 0), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, 0), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, -outlineOffset), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, outlineOffset), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, -outlineOffset), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, -outlineOffset), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, outlineOffset), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, outlineOffset), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);

        // Draw the white text on top with counter-scaling
        spriteBatch.DrawString(debugFont, text, position, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepthTop);
    }
    
    // Updates the polygon creation tool
    private void UpdatePolygonCreation(Matrix invertedCameraMatrix)
    {
        // Get current mouse state
        MouseState currentMouseState = Mouse.GetState();
        
        // Convert screen coordinates to world coordinates for polygon creation
        Vector2 mouseScreenPos = new Vector2(currentMouseState.X, currentMouseState.Y);
        Vector2 mouseWorldPos = Vector2.Transform(mouseScreenPos, invertedCameraMatrix);
        
        // Check if left mouse button was just pressed (not held)
        if (currentMouseState.LeftButton == ButtonState.Pressed && !previousLeftButtonPressed)
        {
            // Add a new point at the mouse position using world coordinates
            polygonPoints.Add(mouseWorldPos);
            Console.WriteLine($"Added point: ({mouseWorldPos.X:F0}, {mouseWorldPos.Y:F0})");
        }
        
        // Check if right mouse button was just pressed (not held)
        if (currentMouseState.RightButton == ButtonState.Pressed && !previousRightButtonPressed)
        {
            // Remove the last point if there are any
            if (polygonPoints.Count > 0)
            {
                Vector2 removedPoint = polygonPoints[polygonPoints.Count - 1];
                polygonPoints.RemoveAt(polygonPoints.Count - 1);
                Console.WriteLine($"Removed point: ({removedPoint.X:F0}, {removedPoint.Y:F0})");
            }
        }
        
        // Update previous mouse state for next frame
        previousLeftButtonPressed = currentMouseState.LeftButton == ButtonState.Pressed;
        previousRightButtonPressed = currentMouseState.RightButton == ButtonState.Pressed;
    }
    
    // Draws the polygon creation preview
    private void DrawPolygonCreationPreview(SpriteBatch spriteBatch)
    {
        // Draw existing points
        for (int i = 0; i < polygonPoints.Count; i++)
        {
            // Draw point as a small rectangle
            DrawRectangle(spriteBatch, polygonPoints[i] - new Vector2(2, 2), new Vector2(4, 4), Color.Red);
            
            // Draw point index
            DrawOutlinedText(spriteBatch, i.ToString(), polygonPoints[i] + new Vector2(5, 5));
            
            // Draw lines connecting points
            if (i > 0)
            {
                DrawLine(spriteBatch, polygonPoints[i - 1], polygonPoints[i], Color.Yellow, 1f);
            }
        }
        
        // Draw line from last point to first point if we have at least 3 points (to close the polygon)
        if (polygonPoints.Count >= 3)
        {
            DrawLine(spriteBatch, polygonPoints[polygonPoints.Count - 1], polygonPoints[0], Color.Yellow, 1f);
        }
        
        // Draw line from last point to current mouse position
        if (polygonPoints.Count > 0)
        {
            MouseState mouseState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorldPos = UICoordinateHelper.ScreenToWorld(
                mouseScreenPos, 
                graphicsManager.cameraManager
            );
            
            DrawLine(spriteBatch, polygonPoints[polygonPoints.Count - 1], mouseWorldPos, Color.LightGray, 1f);
        }
    }
    
    // Outputs the polygon points in a format that can be used for collision shapes
    private void OutputPolygonPoints()
    {
        if (polygonPoints.Count < 3)
        {
            Console.WriteLine("Not enough points to create a polygon (minimum 3 required)");
            return;
        }
        
        Console.WriteLine("\n=== POLYGON VERTICES ===");
        Console.WriteLine("\"Vertices\": [");
        
        for (int i = 0; i < polygonPoints.Count; i++)
        {
            string comma = i < polygonPoints.Count - 1 ? "," : "";
            Console.WriteLine($"    [ {polygonPoints[i].X:F0}, {polygonPoints[i].Y:F0} ]{comma}");
        }
        
        Console.WriteLine("]");
        Console.WriteLine("========================\n");
    }
}
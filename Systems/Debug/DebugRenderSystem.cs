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

    // Debug toggle flags
    private bool showDebug = true;
    private bool showHitboxes = true;
    private bool showEntityIDs = false;

    public override bool Pausible => false;

    public DebugRenderSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameAssets assets)
    {
        this.debugFont = assets.GetFont("DebugFont");
        this.spriteBatch = spriteBatch;
        
        // Create the pixel texture during initialization
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAction);
    }

    private void HandleAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        // Toggle overall debug rendering
        if (actionEvent.ActionName.Equals("toggle_debug", StringComparison.OrdinalIgnoreCase) 
            && actionEvent.IsStarted)
        {
            showDebug = !showDebug;
            Console.WriteLine($"Debug rendering toggled: {showDebug}");
        }
        // Toggle hitbox rendering
        else if (actionEvent.ActionName.Equals("toggle_hitboxes", StringComparison.OrdinalIgnoreCase)
            && actionEvent.IsStarted)
        {
            showHitboxes = !showHitboxes;
            Console.WriteLine($"Hitbox rendering toggled: {showHitboxes}");
        }
        // Toggle entity ID rendering
        else if (actionEvent.ActionName.Equals("toggle_entity_ids", StringComparison.OrdinalIgnoreCase)
            && actionEvent.IsStarted)
        {
            showEntityIDs = !showEntityIDs;
            Console.WriteLine($"Entity ID rendering toggled: {showEntityIDs}");
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        if (!showDebug)
            return; // Skip drawing debug info if overall debug mode is disabled

        // Calculate frames per second
        CalculateFPS(gameTime);

        // Draw Acceleration Vectors (in Red)
        DrawAccelerationVectors(spriteBatch);

        // Draw Velocity Vectors (in Green)
        DrawVelocityVectors(spriteBatch);

        // Conditionally draw hitboxes if enabled
        if (showHitboxes)
            DrawHitboxes(spriteBatch);

        // Draw the FPS Counter using the outlined text helper
        DrawFPSCounter(spriteBatch);

        // Draw player state text above players' heads
        DrawPlayerStateText(spriteBatch);

        // Conditionally draw entity IDs if enabled
        if (showEntityIDs)
            DrawEntityIDs(spriteBatch);

        frameCounter++;
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
        // Draw at a fixed position (e.g., top left)
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

    private void DrawVector(SpriteBatch spriteBatch, Vector2 origin, Vector2 vectorValue, Color color, float scaleFactor)
    {
        Vector2 endPoint = origin + vectorValue * scaleFactor;
        // Draw the vector line
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
            // Center text above the player's head (40 pixels above)
            Vector2 textPosition = position.Value - new Vector2(textSize.X / 2, 40 + textSize.Y);
            DrawOutlinedText(spriteBatch, stateText, textPosition);
        }
    }

    // Draw hitboxes for entities with a CollisionShape and Position component
    private void DrawHitboxes(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || !HasComponents<CollisionShape>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var collisionShape = ref GetComponent<CollisionShape>(entity);

            Vector2 scale = Vector2.One;
            if (HasComponents<Scale>(entity))
                scale = GetComponent<Scale>(entity).Value;

            Vector2 scaledOffset = collisionShape.Offset * scale;
            Vector2 scaledSize = collisionShape.Size * scale;
            Vector2 hitboxPosition = position.Value + scaledOffset;

            if (collisionShape.Type == ShapeType.Rectangle)
            {
                DrawRectangle(spriteBatch, hitboxPosition, scaledSize, Color.Yellow);
            }
            else if (collisionShape.Type == ShapeType.Line)
            {
                DrawLine(spriteBatch, hitboxPosition, hitboxPosition + scaledSize, Color.Yellow, 1f);
            }
        }
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

    // Draws entity IDs centered on the entity's position
    private void DrawEntityIDs(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            // Use the entity's identifier
            string idText = entity.Id.ToString();
            Vector2 textSize = debugFont.MeasureString(idText);
            Vector2 textPosition = position.Value - textSize / 2;
            DrawOutlinedText(spriteBatch, idText, textPosition);
        }
    }

    // Helper method to draw outlined text
    private void DrawOutlinedText(SpriteBatch spriteBatch, string text, Vector2 position)
    {
        float outlineOffset = 2f;
        // Draw the black outline in 8 directions
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, 0), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, 0), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, -outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(0, outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, -outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, -outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(-outlineOffset, outlineOffset), Color.Black);
        spriteBatch.DrawString(debugFont, text, position + new Vector2(outlineOffset, outlineOffset), Color.Black);

        // Draw the white text on top
        spriteBatch.DrawString(debugFont, text, position, Color.White);
    }
}

using ECS.Components.Physics;

public class DebugRenderSystem : SystemBase
{
    private readonly SpriteBatch spriteBatch;
    private Texture2D pixel;
    private SpriteFont debugFont;
    private int frameRate = 0;
    private int frameCounter = 0;
    private TimeSpan elapsedTime = TimeSpan.Zero;
    public override bool Pausible => false;

    public DebugRenderSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, SpriteFont font)
    {
        this.spriteBatch = spriteBatch;
        this.debugFont = font;
        
        // Create the pixel texture during initialization
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Calculate frames per second
        CalculateFPS(gameTime);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw Acceleration Vectors (in Red)
        DrawAccelerationVectors(spriteBatch);

        // Draw Velocity Vectors (in Green)
        DrawVelocityVectors(spriteBatch);

        // Draw the FPS Counter
        DrawFPSCounter(spriteBatch);

        spriteBatch.End();

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

    private void DrawFPSCounter(SpriteBatch spritebatch)
    {
        spriteBatch.DrawString(
            debugFont, 
            $"FPS: {frameRate}", 
            new Vector2(10, 10), 
            Color.White
        );
    }

    private void DrawAccelerationVectors(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            // Check if entity has both Position and Force components
            if (!HasComponents<Position>(entity) || 
                !HasComponents<Acceleration>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var acceleration = ref GetComponent<Acceleration>(entity);

            // Skip if force is zero
            if (acceleration.Value == Vector2.Zero)
                continue;

            DrawVector(spriteBatch, position.Value, acceleration.Value, Color.Red, 0.1f);
        }
    }

    private void DrawVelocityVectors(SpriteBatch spriteBatch)
    {
        foreach (var entity in World.GetEntities())
        {
            // Check if entity has both Position and Velocity components
            if (!HasComponents<Position>(entity) || 
                !HasComponents<Velocity>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            // Skip if velocity is zero
            if (velocity.Value == Vector2.Zero)
                continue;

            DrawVector(spriteBatch, position.Value, velocity.Value, Color.Green, 0.1f);
        }
    }

    private void DrawVector(SpriteBatch spriteBatch, Vector2 origin, Vector2 vectorValue, Color color, float scaleFactor)
    {
        Vector2 endPoint = origin + vectorValue * scaleFactor;

        // Calculate the angle and length of the vector
        float angle = (float)Math.Atan2(vectorValue.Y, vectorValue.X);

        // Draw the line of the vector
        DrawLine(spriteBatch, origin, endPoint, color, 2f);

        // Maybe arrowhead coming soon
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
}
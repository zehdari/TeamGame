namespace ECSAttempt.Systems;
public class RenderSystem : SystemBase
{
    private readonly SpriteBatch spriteBatch;

    public RenderSystem(SpriteBatch spriteBatch)
    {
        this.spriteBatch = spriteBatch;
    }

    public override void Update(World world, GameTime gameTime)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || 
                !HasComponents<Rotation>(entity) ||
                !HasComponents<Scale>(entity) ||
                !HasComponents<SpriteConfig>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var rotation = ref GetComponent<Rotation>(entity);
            ref var scale = ref GetComponent<Scale>(entity);
            ref var sprite = ref GetComponent<SpriteConfig>(entity);

            var roundedPosition = new Vector2((int)position.Value.X, (int)position.Value.Y);

            var spriteEffects = SpriteEffects.None;
            if (HasComponents<FacingDirection>(entity))
            {
                ref var facing = ref GetComponent<FacingDirection>(entity);
                spriteEffects = facing.IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            spriteBatch.Draw(
                sprite.Texture,
                roundedPosition,
                sprite.SourceRect,
                sprite.Color,
                rotation.Value,
                sprite.Origin,
                2.0f,
                spriteEffects,
                0
            );
        }
        
        spriteBatch.End();
    }
}
using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Animation;

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
                !HasComponents<SpriteConfig>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var rotation = ref GetComponent<Rotation>(entity);
            ref var sprite = ref GetComponent<SpriteConfig>(entity);

            var roundedPosition = new Vector2((int)position.Value.X, (int)position.Value.Y);

            var spriteEffects = SpriteEffects.None;
            if (HasComponents<FacingDirection>(entity))
            {
                ref var facing = ref GetComponent<FacingDirection>(entity);
                spriteEffects = facing.IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            Vector2 scale = Vector2.One;
            if (HasComponents<Scale>(entity))
            {
                ref var scaleComponent = ref GetComponent<Scale>(entity);
                scale = scaleComponent.Value;
            }

            spriteBatch.Draw(
                sprite.Texture,
                roundedPosition,
                sprite.SourceRect,
                sprite.Color,
                rotation.Value,
                sprite.Origin,
                scale,
                spriteEffects,
                0
            );
        }

        spriteBatch.End();
    }
}
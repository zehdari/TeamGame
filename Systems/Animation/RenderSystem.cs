using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Animation;

public class RenderSystem : SystemBase
{
    private readonly SpriteBatch spriteBatch;
    private List<Entity> renderQueue = new();
    public override bool Pausible => false;

    public RenderSystem(SpriteBatch spriteBatch)
    {
        this.spriteBatch = spriteBatch;
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Clear and fill render queue
        renderQueue.Clear();
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) ||
                !HasComponents<SpriteConfig>(entity))
                continue;

            renderQueue.Add(entity);
        }

        // Sort entities by draw layer
        renderQueue.Sort((a, b) =>
        {
            var spriteA = GetComponent<SpriteConfig>(a);
            var spriteB = GetComponent<SpriteConfig>(b);
            return spriteA.Layer.CompareTo(spriteB.Layer);
        });

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw entities in sorted order
        foreach (var entity in renderQueue)
        {

            ref var position = ref GetComponent<Position>(entity);
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

            float rotation = 0f;
            if (HasComponents<Rotation>(entity))
            {
                ref var rotationComponent = ref GetComponent<Rotation>(entity);
                rotation = rotationComponent.Value;
            }

            spriteBatch.Draw(
                sprite.Texture,
                roundedPosition,
                sprite.SourceRect,
                sprite.Color,
                rotation,
                sprite.Origin,
                scale,
                spriteEffects,
                0
            );
        }

        spriteBatch.End();
    }
}
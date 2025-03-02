using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.UI;

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
        // Ensure all animations are updated before rendering
        foreach (var entity in World.GetEntities())
        {
            if (HasComponents<AnimationState>(entity) && 
                HasComponents<SpriteConfig>(entity) && 
                HasComponents<AnimationConfig>(entity))
            {
                ref var state = ref GetComponent<AnimationState>(entity);
                ref var sprite = ref GetComponent<SpriteConfig>(entity);
                ref var config = ref GetComponent<AnimationConfig>(entity);

                if (!state.IsPlaying || !config.States.ContainsKey(state.CurrentState))
                    continue;

                var frames = config.States[state.CurrentState];

                // Force an immediate update if frame changed
                sprite.SourceRect = frames[state.FrameIndex].SourceRect;
            }
        }

        // Now render the updated sprites
        renderQueue.Clear();
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || !HasComponents<SpriteConfig>(entity))
                continue;
            //only render sprites that should be during current pause state
            if (HasComponents<UIPaused>(entity))
            {
                ref var UIPaused = ref GetComponent<UIPaused>(entity);
                if (GameStateHelper.IsPaused(World) != UIPaused.Value)
                    continue;
            }

            renderQueue.Add(entity);
        }

        // Sort entities by draw layer
        renderQueue.Sort((a, b) =>
        {
            var spriteA = GetComponent<SpriteConfig>(a);
            var spriteB = GetComponent<SpriteConfig>(b);
            return spriteA.Layer.CompareTo(spriteB.Layer);
        });

        // Draw entities in sorted order
        foreach (var entity in renderQueue)
        {
            ref var position = ref GetComponent<Position>(entity);
            ref var sprite = ref GetComponent<SpriteConfig>(entity);

            var drawPosition = position.Value;

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
                drawPosition,
                sprite.SourceRect,
                sprite.Color,
                rotation,
                sprite.Origin,
                scale,
                spriteEffects,
                0
            );
        }
    }
}
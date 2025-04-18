using ECS.Components.Animation;
using ECS.Core.Debug;

namespace ECS.Systems.Animation;

public class AnimationSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<AnimationStateEvent>(HandleAnimationStateChange);
    }

    private void HandleAnimationStateChange(IEvent evt)
    {
        var animEvent = (AnimationStateEvent)evt;
        if (HasComponents<AnimationState>(animEvent.Entity))
        {
            ref var state = ref GetComponent<AnimationState>(animEvent.Entity);
            if (state.CurrentState != animEvent.NewState)
            {
                state.CurrentState = animEvent.NewState;
                state.FrameIndex = 0;
                state.TimeInFrame = 0;
                state.IsPlaying = true;
            }
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Should always be positive if done right
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Shouldn't happen
        if (deltaTime < 0)
        {
            Logger.Log("Delta time gave a negative time change");
            return;
        }

        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<AnimationState>(entity) ||
                !HasComponents<SpriteConfig>(entity) ||
                !HasComponents<AnimationConfig>(entity))
                continue;

            ref var state = ref GetComponent<AnimationState>(entity);
            ref var sprite = ref GetComponent<SpriteConfig>(entity);
            ref var config = ref GetComponent<AnimationConfig>(entity);

            if (!state.IsPlaying || !config.States.ContainsKey(state.CurrentState))
                continue;

            var frames = config.States[state.CurrentState];
            var currentFrame = frames[state.FrameIndex];

            state.TimeInFrame += deltaTime;

            if (state.TimeInFrame >= currentFrame.Duration)
            {
                state.TimeInFrame = 0;
                
                // Check if we're at the last frame
                bool isLastFrame = state.FrameIndex == frames.Length - 1;
                
                // Handle animation based on Loop and HoldLastFrame settings
                if (isLastFrame && !currentFrame.Loop)
                {
                    if (currentFrame.HoldLastFrame)
                    {
                        // For non-looping animations that hold last frame, just stop playing
                        state.IsPlaying = false;
                    }
                    else
                    {
                        // For non-looping animations that don't hold last frame, go back to IDLE state
                        Publish(new AnimationStateEvent
                        {
                            Entity = entity,
                            NewState = MAGIC.ANIMATIONSTATE.IDLE
                        });
                    }
                }
                else
                {
                    // For looping animations or when not on last frame, advance to next frame
                    state.FrameIndex = (state.FrameIndex + 1) % frames.Length;
                    sprite.SourceRect = frames[state.FrameIndex].SourceRect;
                }
            }
        }
    }
}
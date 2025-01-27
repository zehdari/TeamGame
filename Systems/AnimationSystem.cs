namespace ECS.Systems;

public class AnimationSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<AnimationStateEvent>(HandleAnimationStateChange);
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
            }
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                state.FrameIndex = (state.FrameIndex + 1) % frames.Length;
                sprite.SourceRect = frames[state.FrameIndex].SourceRect;
            }
        }
    }
}
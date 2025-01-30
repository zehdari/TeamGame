namespace ECS.Core;

public class EntityFactory
{
    private readonly World world;

    public EntityFactory(World world)
    {
        this.world = world;
    }

    public Entity CreatePlayer(Texture2D spriteSheet, AnimationConfig animConfig, InputConfig inputConfig)
    {
        var entity = world.CreateEntity();

        world.GetPool<PlayerTag>().Set(entity, new PlayerTag());

        world.GetPool<Position>().Set(entity, new Position
        {
            Value = new Vector2(100, 100)
        });

        world.GetPool<Rotation>().Set(entity, new Rotation
        {
            Value = 0f
        });

        world.GetPool<Scale>().Set(entity, new Scale
        {
            Value = Vector2.One
        });

        world.GetPool<Velocity>().Set(entity, new Velocity
        {
            Value = Vector2.Zero
        });

        world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig
        {
            Texture = spriteSheet,
            SourceRect = animConfig.States["idle"][0].SourceRect,
            Origin = new Vector2(16, 16),
            Color = Color.White
        });

        world.GetPool<AnimationState>().Set(entity, new AnimationState
        {
            CurrentState = "idle",
            TimeInFrame = 0,
            FrameIndex = 0,
            IsPlaying = true
        });

        world.GetPool<FacingDirection>().Set(entity, new FacingDirection
        {
            IsFacingLeft = false
        });

        world.GetPool<AnimationConfig>().Set(entity, animConfig);

        // Set up input configuration and state
        world.GetPool<InputConfig>().Set(entity, inputConfig);
        world.GetPool<InputState>().Set(entity, new InputState
        {
            AxisValues = new Dictionary<string, float>
            {
                { "horizontal", 0f },
                { "vertical", 0f }
            }
        });

        world.GetPool<Force>().Set(entity, new Force
        {
            Value = Vector2.Zero
        });

        world.GetPool<MovementForce>().Set(entity, new MovementForce
        {
            Magnitude = 4000f
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity
        {
            Value = 400f
        });

        return entity;
    }

    public Entity CreateProjectile(Texture2D spriteSheet, AnimationConfig animConfig)
    {
        var entity = world.CreateEntity();

        world.GetPool<ProjectileTag>().Set(entity, new ProjectileTag{ });

        world.GetPool<ExistedTooLong>().Set(entity, new ExistedTooLong
        {
            Value = false
        });

        world.GetPool<Timer>().Set(entity, new Timer
        {
            Duration = 2f,
            Elapsed = 0f
        });

        world.GetPool<Direction>().Set(entity, new Direction
        {
            Value = new Vector2(1, 0)
        });

        world.GetPool<Position>().Set(entity, new Position
        {
            Value = new Vector2(200, 200)
        });

        world.GetPool<Rotation>().Set(entity, new Rotation
        {
            Value = 0f
        });

        world.GetPool<Scale>().Set(entity, new Scale
        {
            Value = Vector2.One
        });

        world.GetPool<Velocity>().Set(entity, new Velocity
        {
            Value = Vector2.Zero
        });

        world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig
        {
            Texture = spriteSheet,
            SourceRect = animConfig.States["idle"][0].SourceRect,
            Origin = new Vector2(16, 16),
            Color = Color.White
        });

        world.GetPool<AnimationConfig>().Set(entity, animConfig);

        world.GetPool<AnimationState>().Set(entity, new AnimationState
        {
            CurrentState = "idle",
            TimeInFrame = 0,
            FrameIndex = 0,
            IsPlaying = true
        });

        world.GetPool<FacingDirection>().Set(entity, new FacingDirection
        {
            IsFacingLeft = false
        });

        world.GetPool<Force>().Set(entity, new Force
        {
            Value = Vector2.Zero
        });

        world.GetPool<MovementForce>().Set(entity, new MovementForce
        {
            Magnitude = 4000f
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity
        {
            Value = 4000f
        });

        return entity;
    }

    public Entity CreateEnemy(Texture2D spriteSheet, AnimationConfig animConfig)
    {
        var entity = world.CreateEntity();

        world.GetPool<AITag>().Set(entity, new AITag { });

        world.GetPool<RandomRange>().Set(entity, new RandomRange
        {
            Minimum = 0,
            Maximum = 1
        });

        world.GetPool<RandomlyGeneratedFloat>().Set(entity, new RandomlyGeneratedFloat
        {
            Value = 0
        });

        world.GetPool<Timer>().Set(entity, new Timer
        {
            Duration = 1f,
            Elapsed = 0f
        });

        world.GetPool<Direction>().Set(entity, new Direction
        {
            Value = new Vector2(1, 0)
        });

        world.GetPool<Position>().Set(entity, new Position
        {
            Value = new Vector2(420, 240)
        });

        world.GetPool<Rotation>().Set(entity, new Rotation
        {
            Value = 0f
        });

        world.GetPool<Scale>().Set(entity, new Scale
        {
            Value = Vector2.One
        });

        world.GetPool<Velocity>().Set(entity, new Velocity
        {
            Value = Vector2.Zero
        });

        world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig
        {
            Texture = spriteSheet,
            SourceRect = animConfig.States["idle"][0].SourceRect,
            Origin = new Vector2(16, 16),
            Color = Color.White
        });

        world.GetPool<AnimationConfig>().Set(entity, animConfig);

        world.GetPool<AnimationState>().Set(entity, new AnimationState
        {
            CurrentState = "idle",
            TimeInFrame = 0,
            FrameIndex = 0,
            IsPlaying = true
        });

        world.GetPool<FacingDirection>().Set(entity, new FacingDirection
        {
            IsFacingLeft = false
        });

        world.GetPool<Force>().Set(entity, new Force
        {
            Value = Vector2.Zero
        });

        world.GetPool<MovementForce>().Set(entity, new MovementForce
        {
            Magnitude = 1000f
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity
        {
            Value = 400f
        });

        return entity;
    }
}
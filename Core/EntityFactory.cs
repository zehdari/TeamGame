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

        world.GetPool<Mass>().Set(entity, new Mass 
        { 
            Value = 1f
        });

        world.GetPool<Acceleration>().Set(entity, new Acceleration
        {
            Value = Vector2.Zero
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

        // Gravity Code Here
        world.GetPool<GravitySpeed>().Set(entity, new GravitySpeed
        {
            Value = new Vector2(0f, 30f)
        });

        world.GetPool<MovementForce>().Set(entity, new MovementForce 
        {
            Magnitude = 2000f 
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<AirResistance>().Set(entity, new AirResistance 
        {
            Value = 0.1f 
        });

        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity 
        {
            Value = 400f
        });

        world.GetPool<CollisionShape>().Set(entity, new CollisionShape
        {
            Type = ShapeType.Rectangle,
            Size = new Vector2(26, 20),  // Slightly larger than sprite (will use parsing instead of magic num later)
            Offset = new Vector2(-13, -10),  // Center the collision box (same ^)
            IsPhysical = true,
            IsOneWay = false
        });

        world.GetPool<CollisionState>().Set(entity, new CollisionState
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
        });

        world.GetPool<IsGrounded>().Set(entity, new IsGrounded
        {
            Value = false
        });

        return entity;
    }

    public Entity CreateFloor(Vector2 position, Vector2 size)
    {
        var entity = world.CreateEntity();

        world.GetPool<Position>().Set(entity, new Position 
        { 
            Value = position
        });

        world.GetPool<CollisionShape>().Set(entity, new CollisionShape
        {
            Type = ShapeType.Rectangle,
            Size = size,
            Offset = new Vector2(-size.X / 2, -size.Y / 2),  // Center the collision box
            IsPhysical = true,
            IsOneWay = false
        });

        world.GetPool<CollisionState>().Set(entity, new CollisionState
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
        });

        return entity;
    }

    public Entity CreatePlatform(Vector2 position, Vector2 size, bool isOneWay = true)
    {
        var entity = world.CreateEntity();

        world.GetPool<Position>().Set(entity, new Position 
        { 
            Value = position
        });

        world.GetPool<CollisionShape>().Set(entity, new CollisionShape
        {
            Type = ShapeType.Rectangle,
            Size = size,
            Offset = new Vector2(-size.X / 2, -size.Y / 2),  // Center the collision box
            IsPhysical = true,
            IsOneWay = isOneWay  // Flag is there, need to implement still
        });

        world.GetPool<CollisionState>().Set(entity, new CollisionState
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
        });

        return entity;
    }

    public Entity CreateLine(Vector2 start, Vector2 end)
    {
        var entity = world.CreateEntity();

        world.GetPool<Position>().Set(entity, new Position 
        { 
            Value = start
        });

        world.GetPool<CollisionShape>().Set(entity, new CollisionShape
        {
            Type = ShapeType.Line,
            Size = end - start,
            Offset = Vector2.Zero,
            IsPhysical = true,
            IsOneWay = false
        });

        world.GetPool<CollisionState>().Set(entity, new CollisionState
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
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

        world.GetPool<Mass>().Set(entity, new Mass
        {
            Value = 1f
        });

        world.GetPool<Acceleration>().Set(entity, new Acceleration
        {
            Value = Vector2.Zero
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
            Magnitude = 1000f
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<AirResistance>().Set(entity, new AirResistance
        {
            Value = 0.1f
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity
        {
            Value = 4000f
        });

        world.GetPool<CollisionShape>().Set(entity, new CollisionShape
        {
            Type = ShapeType.Rectangle,
            Size = new Vector2(26, 20),  // Slightly larger than sprite (will use parsing instead of magic num later)
            Offset = new Vector2(-13, -10),  // Center the collision box (same ^)
            IsPhysical = true,
            IsOneWay = false
        });

        world.GetPool<CollisionState>().Set(entity, new CollisionState
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
        });

        return entity;
    }

    public Entity CreateEnemy(Texture2D spriteSheet, AnimationConfig animConfig)
    {
        var entity = world.CreateEntity();

        world.GetPool<AITag>().Set(entity, new AITag());

        world.GetPool<Timer>().Set(entity, new Timer
        {
            Duration = 1f,
            Elapsed = 0f
        });

        world.GetPool<RandomRange>().Set(entity, new RandomRange
        {
            Maximum = 3,
            Minimum = 0
        });

        world.GetPool<RandomlyGeneratedInteger>().Set(entity, new RandomlyGeneratedInteger
        {
            Value = 0
        });

        world.GetPool<Direction>().Set(entity, new Direction
        {
            Value = new Vector2(1, 0)
        });

        world.GetPool<CurrentAction>().Set(entity, new CurrentAction
        {
            Value = "jump"
        });

        world.GetPool<JumpSpeed>().Set(entity, new JumpSpeed
        {
            Value = new Vector2(0, -3000)
        });

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

        world.GetPool<Mass>().Set(entity, new Mass
        {
            Value = 1f
        });

        world.GetPool<Acceleration>().Set(entity, new Acceleration
        {
            Value = Vector2.Zero
        });

        world.GetPool<AnimationConfig>().Set(entity, animConfig);

        world.GetPool<Force>().Set(entity, new Force
        {
            Value = Vector2.Zero
        });

        // Gravity Code Here
        world.GetPool<GravitySpeed>().Set(entity, new GravitySpeed
        {
            Value = new Vector2(0f, 30f)
        });

        world.GetPool<MovementForce>().Set(entity, new MovementForce
        {
            Magnitude = 2000f
        });

        world.GetPool<Friction>().Set(entity, new Friction
        {
            Value = 15f
        });

        world.GetPool<AirResistance>().Set(entity, new AirResistance
        {
            Value = 0.1f
        });

        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity
        {
            Value = 400f
        });

        world.GetPool<CollisionShape>().Set(entity, new CollisionShape
        {
            Type = ShapeType.Rectangle,
            Size = new Vector2(26, 20),  // Slightly larger than sprite (will use parsing instead of magic num later)
            Offset = new Vector2(-13, -10),  // Center the collision box (same ^)
            IsPhysical = true,
            IsOneWay = false
        });

        world.GetPool<CollisionState>().Set(entity, new CollisionState
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
        });

        world.GetPool<IsGrounded>().Set(entity, new IsGrounded
        {
            Value = false
        });

        return entity;
    }
}
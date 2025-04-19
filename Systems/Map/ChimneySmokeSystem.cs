using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Objects;
using ECS.Components.Timer;
using ECS.Components.Physics;
using ECS.Components.Tags;

namespace ECS.Systems.Effects;

public class ChimneySmokeSystem : SystemBase
{
    // Dictionaries for state transitions and animation states
    private readonly Dictionary<SmokeState, SmokeState> nextStateMap;
    private readonly Dictionary<SmokeState, string> animationStateMap;
    
    public ChimneySmokeSystem()
    {
        // Initialize state transition dictionary
        nextStateMap = new Dictionary<SmokeState, SmokeState>
        {
            { SmokeState.Hidden, SmokeState.NightChimney },
            { SmokeState.NightChimney, SmokeState.Chimney },
            { SmokeState.Chimney, SmokeState.Hidden }
        };
        
        // Initialize animation state mapping dictionary
        // THESE NEED REPLACED WITH ACTUAL SPRITES AND MAGIC.
        // HIDDEN JUST NEEDS MAGIC.
        animationStateMap = new Dictionary<SmokeState, string>
        {
            { SmokeState.Hidden, "hidden" },
            { SmokeState.NightChimney, "smoke" },
            { SmokeState.Chimney, "smoke" }
        };
    }
    
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerExpired);
        Subscribe<CollisionEvent>(HandleCollision);
    }
    
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in world.GetEntities())
        {
            // Skip any entity that doesn't have the required components
            if (!HasComponents<Smoke>(entity) || 
                !HasComponents<AnimationState>(entity) ||
                !HasComponents<CollisionBody>(entity))
                continue;
                
            // Initialize timer if needed
            InitializeTimerIfNeeded(entity);
        }
    }
    
    private void InitializeTimerIfNeeded(Entity entity)
    {
        // Ensure entity has a timers component
        if (!HasComponents<Timers>(entity))
        {
            World.GetPool<Timers>().Set(entity, new Timers 
            { 
                TimerMap = new Dictionary<TimerType, Timer>() 
            });
        }
        
        // Get the timers component
        ref var timers = ref GetComponent<Timers>(entity);
        
        // Initialize the timer map if needed
        if (timers.TimerMap == null)
        {
            timers.TimerMap = new Dictionary<TimerType, Timer>();
        }
        
        // Check if smoke timer already exists
        if (!timers.TimerMap.ContainsKey(TimerType.MapTimer))
        {
            // Get current state and its duration
            ref var smoke = ref GetComponent<Smoke>(entity);
            float duration = GetDurationForState(entity, smoke.CurrentState);
            
            // Create and add the timer
            timers.TimerMap[TimerType.MapTimer] = new Timer
            {
                Duration = duration,
                Elapsed = 0f,
                Type = TimerType.MapTimer,
                OneShot = true
            };
        }
    }
    
    private void HandleTimerExpired(IEvent evt)
    {
        if (!(evt is TimerEvent timerEvent) || timerEvent.TimerType != TimerType.MapTimer)
            return;
            
        Entity entity = timerEvent.Entity;
        
        // Check if this is a smoke entity
        if (!HasComponents<Smoke>(entity))
            return;
            
        CycleSmokeState(entity);
    }
    
    private void HandleCollision(IEvent evt)
    {
        var collision = (CollisionEvent)evt;
        
        // Only process collision begin events
        if (collision.EventType != CollisionEventType.Begin)
            return;
        
        // Identify smoke vs character
        Entity? smokeEntity = null;
        Entity? characterEntity = null;
        
        if (HasComponents<Smoke>(collision.Contact.EntityA))
        {
            smokeEntity = collision.Contact.EntityA;
            characterEntity = collision.Contact.EntityB;
        }
        else if (HasComponents<Smoke>(collision.Contact.EntityB))
        {
            smokeEntity = collision.Contact.EntityB;
            characterEntity = collision.Contact.EntityA;
        }
        
        // Skip if not a smoke-character collision
        if (smokeEntity == null || characterEntity == null || 
            !HasComponents<CharacterTag>(characterEntity.Value))
            return;
        
        // Check smoke is in Chimney state (the only state where collision is active)
        ref var smoke = ref GetComponent<Smoke>(smokeEntity.Value);
        if (smoke.CurrentState != SmokeState.Chimney)
            return;
        
        // Ensure character has a Force component
        if (!HasComponents<Force>(characterEntity.Value))
        {
            World.GetPool<Force>().Set(characterEntity.Value, new Force());
        }
        
        // Apply force to character
        ref var force = ref GetComponent<Force>(characterEntity.Value);
        force.Value += smoke.ForceToApply;
    }
    
    private string GetAnimationStateFromSmokeState(SmokeState state)
    {
        return animationStateMap[state];
    }
    
    private float GetDurationForState(Entity entity, SmokeState state)
    {
        ref var smoke = ref GetComponent<Smoke>(entity);
        
        var durationMap = new Dictionary<SmokeState, float>
        {
            { SmokeState.Chimney, smoke.ChimneyDuration },
            { SmokeState.NightChimney, smoke.NightChimneyDuration },
            { SmokeState.Hidden, smoke.HiddenDuration }
        };
        
        return durationMap[state];
    }
    
    private SmokeState GetNextState(SmokeState currentState)
    {
        return nextStateMap[currentState];
    }
    
    private void CycleSmokeState(Entity entity)
    {
        // Get current smoke component
        ref var smoke = ref GetComponent<Smoke>(entity);
        
        // Calculate next state
        SmokeState nextState = GetNextState(smoke.CurrentState);
        
        // Update smoke state
        smoke.CurrentState = nextState;
        
        // Update animation state
        ref var animState = ref GetComponent<AnimationState>(entity);
        animState.CurrentState = GetAnimationStateFromSmokeState(nextState);
        animState.FrameIndex = 0;
        animState.TimeInFrame = 0;
        
        // Update collision properties - only enable collision for Chimney state
        ref var collisionBody = ref GetComponent<CollisionBody>(entity);
        if (collisionBody.Polygons != null && collisionBody.Polygons.Count > 0)
        {
            // Update all polygons in the collision body
            for (int i = 0; i < collisionBody.Polygons.Count; i++)
            {
                var polygon = collisionBody.Polygons[i];
                polygon.CollidesWith = nextState == SmokeState.Chimney ? 
                    CollisionLayer.Hitbox : CollisionLayer.None;
                collisionBody.Polygons[i] = polygon;
            }
        }
        
        // Reset timer with new duration
        ref var timers = ref GetComponent<Timers>(entity);
        timers.TimerMap[TimerType.MapTimer] = new Timer
        {
            Duration = GetDurationForState(entity, nextState),
            Elapsed = 0f,
            Type = TimerType.MapTimer,
            OneShot = true
        };
    }
}
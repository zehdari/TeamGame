using ECS.Components.AI;
using ECS.Components.Physics;
using ECS.Components.Random;
using ECS.Components.Tags;
using ECS.Components.Timer;
using ECS.Core.Debug;

namespace ECS.Systems.AI;

public class AISystem : SystemBase
{
    private const float DEFAULT_AI_TIMER_DURATION = 1.0f; // Default duration for the AI timer (This will be moved)
    // Mapping actions to ints
    private Dictionary<int, string> actions = new();

    private void MappingSetter()
    {
        int i = 0;
        actions.Add(i++, MAGIC.ACTIONS.JUMP);
        actions.Add(i++, MAGIC.ACTIONS.WALKLEFT);
        actions.Add(i++, MAGIC.ACTIONS.WALKRIGHT);
        actions.Add(i++, MAGIC.ACTIONS.SHOOT);
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerUp);
        MappingSetter();
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        // Only process events for the AITimer
        if (timerEvent.TimerType != TimerType.AITimer)
            return;

        // Ensure the entity has the necessary AI components
        if (!HasComponents<AITag>(timerEvent.Entity) ||
            !HasComponents<CurrentAction>(timerEvent.Entity) ||
            !HasComponents<RandomlyGeneratedInteger>(timerEvent.Entity))
            return;

        // Retrieve components
        ref var action = ref GetComponent<CurrentAction>(timerEvent.Entity);
        ref var randomInt = ref GetComponent<RandomlyGeneratedInteger>(timerEvent.Entity);

        // Publish an ActionEvent indicating the previous action is released
        World.EventBus.Publish(new ActionEvent
        {
            ActionName = action.Value,
            Entity = timerEvent.Entity,
            IsStarted = false,
            IsEnded = true,
            IsHeld = false,
        });

        // Choose a new action based on the random integer
        if (actions.TryGetValue(randomInt.Value, out string newAction))
            action.Value = newAction;

        // this shouldnt happen
        if (newAction == null) {
            Logger.Log("AI tried to call an action but it was null");
            return;

            }

        // Publish an ActionEvent indicating the new action is started
        Publish(new ActionEvent
        {
            ActionName = action.Value,
            Entity = timerEvent.Entity,
            IsStarted = true,
            IsEnded = false,
            IsHeld = true,
        });
    }

    public override void Update(World world, GameTime gameTime)
    {
        // For every AI entity, ensure it has a Timers component and an AITimer entry
        foreach (Entity entity in world.GetEntities())
        {
            if (!HasComponents<AITag>(entity))
                continue;

            // Ensure the entity has a Timers component
            if (!HasComponents<Timers>(entity))
            {
                World.GetPool<Timers>().Set(entity, new Timers
                {
                    TimerMap = new Dictionary<TimerType, Timer>()
                });
            }

            // Get the Timers component
            ref var timers = ref GetComponent<Timers>(entity);

            // If the AITimer isn't already added, add it
            if (!timers.TimerMap.ContainsKey(TimerType.AITimer))
            {
                timers.TimerMap[TimerType.AITimer] = new Timer
                {
                    Duration = DEFAULT_AI_TIMER_DURATION,
                    Elapsed = 0f,
                    Type = TimerType.AITimer
                };
            }
        }
    }
}

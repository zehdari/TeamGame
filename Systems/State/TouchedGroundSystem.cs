using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Timer;
using ECS.Components.AI;

namespace ECS.Systems.State;

public class TouchedGroundSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TouchedGroundEvent>(HandleGroundTouch);
    }

  

    // Determines the next state when a timed state ends.
    private void HandleGroundTouch(IEvent evt)
    {
        
    }

    
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<AttackCounts>(entity) ||
                !HasComponents<IsGrounded>(entity)) return;

            ref var isGrounded = ref GetComponent<IsGrounded>(entity);
            System.Diagnostics.Debug.WriteLine($"grounded value is {isGrounded.Value}, returns in false");
            if (!isGrounded.Value) return;

            ref var counts = ref GetComponent<AttackCounts>(entity);

            System.Diagnostics.Debug.WriteLine("Resetting counts");

            // Reset all attackCounts to 0
            var arr = counts.TimesUsed.Keys.ToArray();
            foreach (var str in arr)
            {
                counts.TimesUsed[str] = 0;
            }

        }
    }
}

using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.AI;
using ECS.Components.Physics;

namespace ECS.Systems.Attacking;

public class TouchedGroundSystem : SystemBase
{


    public override void Initialize(World world) {  base.Initialize(world); }


    public override void Update(World world, GameTime gameTime) 
    { 
        foreach(var entity in world.GetEntities())
        {

            if (!HasComponents<AttackCounts>(entity) ||
                !HasComponents<IsGrounded>(entity))
                continue;

            ref var attackCounts = ref GetComponent<AttackCounts>(entity);
            ref var isGrounded = ref GetComponent<IsGrounded>(entity);

            // Return early if not grounded, we don't want to reset any counts
            if (!isGrounded.Value)
                return;


            // Reset all counts to 0 when character is on the ground
            var arr = attackCounts.TimesUsed.Keys.ToArray();
            foreach(var str in arr)
            {
                attackCounts.TimesUsed[str] = 0;
            }

        }
    }
}

using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Events;

namespace ECS.Systems.Hitbox;

public class ProjectileHitSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ProjectileHitEvent>(HandleProjectileHitEvent);
    }
    private void HandleProjectileDespawn(Entity attacker, Entity target)
    {
        // Despawn the projectile
        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = attacker
        });

        // Right now, this is specifically for peas, as we don't have any other projectiles. 
        // This will need to change.
        ref var attackerPosition = ref GetComponent<Position>(target);
        Publish<ProjectileDespawnEvent>(new ProjectileDespawnEvent
        {
            type = "splat_pea",
            hitPoint = attackerPosition.Value,
            World = World
        });
    }

    private bool isCollidingWithParent(Entity attacker, Entity target)
    {
        if (!HasComponents<ParentID>(attacker)) return false;
        ref var attackerParent = ref GetComponent<ParentID>(attacker);
        return attackerParent.Value == target.Id;
    }

    private void HandleProjectileHitEvent(IEvent evt)
    {
        ProjectileHitEvent projectileHitEvent = (ProjectileHitEvent)evt;

        if (isCollidingWithParent(projectileHitEvent.Attacker, projectileHitEvent.Target))
            return;

        // If the target is stunned, we still want to despawn the projectile and play the splat animation,
        // but it should do no knockback/damage.

        ref var state = ref GetComponent<PlayerStateComponent>(projectileHitEvent.Target);
        if (state.CurrentState != PlayerState.Stunned)
        {
            /* Lots of repeated code between AttackHitSystem and this, maybe pull out and make 
             * another event to handle this?
             */
            
            ref var positionTarget = ref GetComponent<Position>(projectileHitEvent.Target);
            ref var positionAttacker = ref GetComponent<Position>(projectileHitEvent.Attacker);
            ref var attackerAttack = ref GetComponent<AttackInfo>(projectileHitEvent.Attacker);

            var currentAttack = attackerAttack.ActiveAttack;

            // Get the current attack struct so we can access the damage, kb, etc
            var attack = attackerAttack.AvailableAttacks.First(attack => attack.Type.Equals(currentAttack));

            // Get the direction vector between attacker and target
            var difference = positionTarget.Value - positionAttacker.Value;
            difference.Normalize();

            Publish<HitEvent>(new HitEvent
            {
                Attacker = projectileHitEvent.Attacker,
                Target = projectileHitEvent.Target,
                Damage = attack.Damage,
                Knockback = attack.Knockback,
                StunDuration = attack.StunDuration,
                ContactPoint = difference
            });
        }

        HandleProjectileDespawn(projectileHitEvent.Attacker, projectileHitEvent.Target);
            
    }
    
    public override void Update(World world, GameTime gameTime) { }
}

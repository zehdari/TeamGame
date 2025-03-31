using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Events;

namespace ECS.Systems.Hitbox;

public class ProjectileHitSystem : SuperHitSystem
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ProjectileHitEvent>(HandleProjectileHitEvent);
    }
    private void HandleProjectileDespawn(Entity attacker, Entity target)
    {
        ref var attackerDespawnType = ref GetComponent<ProjectileDespawnType>(attacker);

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
            type = attackerDespawnType.Value,
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
        if (state.CurrentState != PlayerState.Stunned && !isBlocking(projectileHitEvent.Target))
        {
            SendHitEvent(projectileHitEvent.Attacker, projectileHitEvent.Target);
        }

        HandleProjectileDespawn(projectileHitEvent.Attacker, projectileHitEvent.Target);
            
    }
    
    public override void Update(World world, GameTime gameTime) { }
}

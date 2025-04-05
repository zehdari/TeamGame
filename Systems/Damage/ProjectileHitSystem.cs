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

        // Store the position information for the despawn effect before despawning the projectile
        Vector2 hitPoint = Vector2.Zero;
        string despawnType = "";
        
        // Get despawn info before despawning the projectile
        if (HasComponents<Position>(projectileHitEvent.Target) && 
            HasComponents<ProjectileDespawnType>(projectileHitEvent.Attacker))
        {
            hitPoint = GetComponent<Position>(projectileHitEvent.Target).Value;
            despawnType = GetComponent<ProjectileDespawnType>(projectileHitEvent.Attacker).Value;
        }
        
        // First despawn the projectile to prevent further physics interactions
        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = projectileHitEvent.Attacker
        });
        
        // Then handle hit effects if needed
        ref var state = ref GetComponent<PlayerStateComponent>(projectileHitEvent.Target);
        if (state.CurrentState != PlayerState.Stunned && !isBlocking(projectileHitEvent.Target))
        {
            base.SendHitEvent(projectileHitEvent.Attacker, projectileHitEvent.Target);
        }
        
        // Finally spawn the despawn effect if we have valid data
        if (!string.IsNullOrEmpty(despawnType))
        {
            Publish<ProjectileDespawnEvent>(new ProjectileDespawnEvent
            {
                type = despawnType,
                hitPoint = hitPoint,
                World = World
            });
        }
    }
    
    public override void Update(World world, GameTime gameTime) { }
}

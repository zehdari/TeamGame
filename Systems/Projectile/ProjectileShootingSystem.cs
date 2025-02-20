using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.State;

namespace ECS.Systems.Projectile;

public class ProjectileShootingSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleShootAction);
    }

    private void HandleShootAction(IEvent evt)
    {
        /*
         * Whole idea here, if the player shot, set the flag to true so we can deal with it in update
         * where we have access to world and all that.
         */
        var shootEvent = (ActionEvent)evt;

        if (!shootEvent.ActionName.Equals("shoot"))
            return;

        if(!shootEvent.IsStarted) 
            return;

        if (!HasComponents<ShotProjectile>(shootEvent.Entity))
            return;

        ref var shotProjectile = ref GetComponent<ShotProjectile>(shootEvent.Entity);

        shotProjectile.Value = true;
        
    }

    public override void Update(World world, GameTime gameTime)
    {
        var entities = World.GetEntities();

        foreach (var entity in entities)
        {
            if (!HasComponents<ShotProjectile>(entity))
                continue;
            
            ref var shotProjectile = ref GetComponent<ShotProjectile>(entity);
            ref var animConfig = ref GetComponent<AnimationConfig>(entity);
            ref var spriteConfig = ref GetComponent<SpriteConfig>(entity);
            
            // If this entity shot something...
            if (shotProjectile.Value)
            {
                Publish<SpawnEvent>(new SpawnEvent
                {
                    typeSpawned = "projectile",
                    Entity = entity,
                    World = world
                });

                // Reset the flag after firing
                shotProjectile.Value = false;  
                
                if (!HasComponents<PlayerStateComponent>(entity))
                    return;
                // Check if the attack animation exists
                if (animConfig.States.TryGetValue("attack", out var frames))
                {
                    float totalDuration = 0f;
                    foreach (var frame in frames)
                    {
                        totalDuration += frame.Duration;
                    }

                    Publish(new PlayerStateEvent
                    {
                        Entity = entity,
                        RequestedState = PlayerState.Attack,
                        Force = true,
                        Duration = totalDuration
                    });
                }            
            }  
        }
    }
}
using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Tags;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Projectile;

public class HitboxDespawnSystem : SystemBase
{

    private Stack<Polygon> despawners = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerUp);
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        if (timerEvent.TimerType != TimerType.HitboxTimer)
            return;

        ref var collisionBody = ref GetComponent<CollisionBody>(timerEvent.Entity);

        foreach(var polygon in collisionBody.Polygons)
        {
            if(polygon.Layer == CollisionLayer.Hurtbox)
            {
                despawners.Push(polygon);
            }
        }

        while (despawners.Count > 0)
        {
            Polygon polygon = despawners.Pop();
            collisionBody.Polygons.Remove(polygon);
        }


    }

    public override void Update(World world, GameTime gameTime) 
    {
        
    }
}

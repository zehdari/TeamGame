using ECS.Components.Physics;
using ECS.Components.Map;
using System.Diagnostics;

namespace ECS.Systems.Map;

public class PlatformMoveSystem : SystemBase
{
    private const float PLATFORM_SPEED = 50f;
    private const float POINT_THRESHOLD = 5f;

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<PlatformRoute>(entity) ||
                !HasComponents<Velocity>(entity) ||
                !HasComponents<Position>(entity))
                continue;

            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var route = ref GetComponent<PlatformRoute>(entity);

            if (route.Points.Count == 0)
                continue;

            if (route.CurrentIndex >= route.Points.Count)
                route.CurrentIndex = 0;

            var target = route.Points[route.CurrentIndex];
            var direction = target - position.Value;
            var distance = direction.Length();

            if (distance < POINT_THRESHOLD)
            {
                // Reached the point, go to the next one
                route.CurrentIndex = (route.CurrentIndex + 1) % route.Points.Count;
                target = route.Points[route.CurrentIndex];
                direction = target - position.Value;
                distance = direction.Length();
            }
            if (distance > 0f)
            {
                direction = target - position.Value;
                direction.Normalize();

                //setting velocity directly since giving the platform mass looks like it will make it have gravity.

                //System.Diagnostics.Debug.WriteLine($"Velocity: X = {direction.X}, Y = {direction.Y}");
                velocity.Value = direction * PLATFORM_SPEED;
               // System.Diagnostics.Debug.WriteLine($"Velocity: X = {direction.X}, Y = {direction.Y}");


            }
            else
            {
                velocity.Value = Vector2.Zero;
            }

        }
    }
}

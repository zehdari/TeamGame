namespace ECS.Systems;

public class FacingSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<FacingDirection>(entity) || 
                !HasComponents<Velocity>(entity)) 
                continue;

            ref var facing = ref GetComponent<FacingDirection>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            // Only update facing if there's horizontal movement
            if (velocity.Value.X == 0) continue;

            facing.IsFacingLeft = velocity.Value.X < 0;
        }
    }
}
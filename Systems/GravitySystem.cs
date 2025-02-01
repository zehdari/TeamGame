namespace ECS.Systems;

public class GravitySystem : SystemBase
{
    private const float GRAVITY_ACCELERATION = 1000f;

    private readonly Vector2 gravity = new(0, GRAVITY_ACCELERATION);

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Mass>(entity) || 
                !HasComponents<Force>(entity))
                continue;

            ref var mass = ref GetComponent<Mass>(entity);
            ref var force = ref GetComponent<Force>(entity);

            // F = mg
            force.Value += gravity * mass.Value;
        }
    }
}
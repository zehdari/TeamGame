using ECS.Components.Animation;

namespace ECS.Systems.Animation;

public class FacingSystem : SystemBase
{
    private Dictionary<Entity, bool> isWalkingLeft = new();
    private Dictionary<Entity, bool> isWalkingRight = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleWalkAction);
    }

    private void HandleWalkAction(IEvent evt)
    {
        var walkEvent = (ActionEvent)evt;

        // Only track walk actions
        if (walkEvent.ActionName.Equals("walk_left"))
        {
            if (!isWalkingLeft.ContainsKey(walkEvent.Entity))
                isWalkingLeft[walkEvent.Entity] = false;

            isWalkingLeft[walkEvent.Entity] = walkEvent.IsHeld;
        }

        if (walkEvent.ActionName.Equals("walk_right"))
        {
            if (!isWalkingRight.ContainsKey(walkEvent.Entity))
                isWalkingRight[walkEvent.Entity] = false;

            isWalkingRight[walkEvent.Entity] = walkEvent.IsHeld;
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<FacingDirection>(entity))
                continue;

            ref var facing = ref GetComponent<FacingDirection>(entity);

            // Change facing only based on walk input
            if (isWalkingLeft.TryGetValue(entity, out bool walkingLeft) && walkingLeft)
            {
                facing.IsFacingLeft = true;
            }
            else if (isWalkingRight.TryGetValue(entity, out bool walkingRight) && walkingRight)
            {
                facing.IsFacingLeft = false;
            }
        }
    }
}
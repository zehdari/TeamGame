
public class ProjectileShootingSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleShootAction);
    }

    private void HandleShootAction(IEvent evt)
    {
        /*
         * Whole idea here, if the player shot, set the flag to true so we can deal with it in update
         * where we have access to world and all that
         */
        var shootEvent = (ActionEvent)evt;

        if (!shootEvent.ActionName.Equals("shoot"))
            return;

        if (!HasComponents<ShotProjectile>(shootEvent.Entity))
            return;

        ref var shotProjectile = ref GetComponent<ShotProjectile>(shootEvent.Entity);

        shotProjectile.Value = true;
        
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<ProjectileTag>(entity) ||
                !HasComponents<ShotProjectile>(entity))
                continue;
            
            ref var shotProjectile = ref GetComponent<ShotProjectile>(entity);
            ref var animConfig = ref GetComponent<AnimationConfig>(entity);

            // If this entity shot something...
            if(shotProjectile.Value)
            {

                /*
                 * This is going to be a bit more work than I thought, I need access to all of those spritesheets
                 * and configs.
                 */

                /*
                 * Maybe we actually want to make a new event called like "spawn event"?
                 */

                //Texture2D texture = null;

                //EntityFactory.CreateProjectile(texture, animConfig);
            }


            
        }
    }
}
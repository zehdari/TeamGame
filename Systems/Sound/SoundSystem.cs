//using ;
namespace ECS.Systems.Sound
{
	public class SoundSystem : SystemBase
	{
        private SoundManager soundManager;

        public SoundSystem(SoundManager soundManager)
        {
            this.soundManager = soundManager;
        }
        public override void Initialize(World world)
        {
            base.Initialize(world);
            Subscribe<ProjectileDespawnEvent>(HandleProjectileHit);
            //Subscribe<HitEvent>(HandleHit);
            //Subscribe<DespawnEvent>(HandleDespawn);
        }

        public void HandleProjectileHit(IEvent evt){
            soundManager.Play("Pop");
		}

        public void HandleHit(IEvent evt)           // Not currently used, will likely get rid of later
        {
            soundManager.Play("Punch");
        }

        public void HandleDespawn(IEvent evt)       // Not currently used, will likely get rid of later
        {
            soundManager.Play("Death");
        }

        public override void Update(World world, GameTime gameTime)
        {
            // ---
        }
    }
}


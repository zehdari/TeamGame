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

            Subscribe<SoundEvent>(HandleSoundEvent);
        }

        public void HandleSoundEvent(IEvent evt)
        {
            SoundEvent soundEvent = (SoundEvent)evt;

            try
            {
                if (!soundEvent.isMusic)
                {
                    soundManager.PlaySFX(soundEvent.SoundKey);
                }
                else
                {
                    soundManager.PlayMusic(soundEvent.SoundKey);
                }
                
            }
            catch
            {
                // Play default error sound when sound does not exist
                soundManager.PlaySFX(MAGIC.SOUND.ERROR);
            }

        }

        public override void Update(World world, GameTime gameTime)
        {
            // ---
        }
    }
}


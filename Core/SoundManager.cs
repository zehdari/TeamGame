namespace ECS.Core;

public class SoundManager
{
    public Dictionary<string, SoundEffect> soundEffects;

    public SoundManager(Game game)
	{
        soundEffects = new Dictionary<string, SoundEffect>();
	}

}



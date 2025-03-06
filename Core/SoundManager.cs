namespace ECS.Core;

public class SoundManager
{
    private GameAssets gameAssets;
    public Dictionary<string, SoundEffect> soundEffects;

    public SoundManager(Game game, GameAssets assets)
	{
        soundEffects = new Dictionary<string, SoundEffect>();
        gameAssets = assets;
		Initialize();
	}

    public void Initialize()
    {
        Play("BackgroundMusic");
    }

	public void Play(string key)
	{
		var sound = gameAssets.GetSound(key);
        sound.Play();
	}

}

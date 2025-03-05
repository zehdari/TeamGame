namespace ECS.Core;

public class SoundManager
{
    private GameAssets gameAssets;
    public Dictionary<string, SoundEffect> soundEffects;

    public SoundManager(Game game, GameAssets assets)
	{
        soundEffects = new Dictionary<string, SoundEffect>();
        gameAssets = assets;
	}

    public void Initialize()
    {
        var backgroundMusic = gameAssets.GetSound("BackgroundMusic");
        backgroundMusic.Play();
        Console.WriteLine($"{backgroundMusic}");
    }

}

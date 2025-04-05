namespace ECS.Core;

public class SoundManager
{
    private GameAssets gameAssets;
    public Dictionary<string, SoundEffectInstance> soundEffectInstances;

    private const string error_sound_key = "Error";
    public string ERROR_SOUND_KEY { get { return error_sound_key; } }

    public SoundManager(Game game, GameAssets assets)
	{
        soundEffectInstances = new Dictionary<string, SoundEffectInstance>();
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
        var instance = sound.CreateInstance();
        if (!soundEffectInstances.ContainsKey(key))
        {
            soundEffectInstances.Add(key, instance);
        }
        sound.Play();
    }

    public void Pause(string key)
    {
        soundEffectInstances[key].Pause();
    }

    public void Stop(string key)
    {
        soundEffectInstances[key].Stop();
    }

    public void Resume(string key)
    {
        soundEffectInstances[key].Resume();
    }

}

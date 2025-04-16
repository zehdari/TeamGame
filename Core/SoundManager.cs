namespace ECS.Core;

public class SoundManager
{
    private GameAssets gameAssets;
    public Dictionary<string, SoundEffectInstance> soundEffectInstances;

    public SoundManager(Game game, GameAssets assets)
	{
        soundEffectInstances = new Dictionary<string, SoundEffectInstance>();
        gameAssets = assets;
		Initialize();
	}

    public void Initialize()
    {
        Play(MAGIC.SOUND.MENU);
    }

	public void Play(string key)
	{
        var sound = gameAssets.GetSound(key);
        var instance = sound.CreateInstance();
        if (!soundEffectInstances.ContainsKey(key))
        {
            soundEffectInstances.Add(key, instance);
        }

        soundEffectInstances[key].Volume = MAGIC.SOUND.MUSIC_VOLUME;
        soundEffectInstances[key].Play();
        //sound.Play();
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

namespace ECS.Core;

public class SoundManager
{
    private GameAssets gameAssets;
    public Dictionary<string, SoundEffectInstance> soundEffectInstances;

    public float MusicVol = MAGIC.SOUND.MUSIC_VOLUME;
    public float SfxVol = MAGIC.SOUND.SFX_VOLUME;


    public SoundManager(Game game, GameAssets assets)
	{
        soundEffectInstances = new Dictionary<string, SoundEffectInstance>();
        gameAssets = assets;
		Initialize();
	}

    public void Initialize()
    {
        PlayMusic(MAGIC.SOUND.MENU);
    }

	public void PlayMusic(string key)
	{
        var sound = gameAssets.GetSound(key);
        var instance = sound.CreateInstance();

        StopAll();

        // Add this song to the dictionary
        if (!soundEffectInstances.ContainsKey(key))
        {
            soundEffectInstances.Add(key, instance);
        }

        soundEffectInstances[key].Volume = MusicVol;
        soundEffectInstances[key].IsLooped = true;
        soundEffectInstances[key].Play();

    }

    public void PlaySFX(string key)
    {
        var sound = gameAssets.GetSound(key);
        var instance = sound.CreateInstance();
        if (!soundEffectInstances.ContainsKey(key))
        {
            soundEffectInstances.Add(key, instance);
        }

        soundEffectInstances[key].Volume = SfxVol;
        soundEffectInstances[key].Play();

    }

    public void Pause(string key)
    {
        soundEffectInstances[key].Pause();
    }

    public void PauseAll()
    {
        foreach((string key, SoundEffectInstance value) in soundEffectInstances)
        {
            Pause(key);
        }
    }

    public void Stop(string key)
    {
        soundEffectInstances[key].Stop();
    }

    public void StopAll()
    {
        foreach ((string key, SoundEffectInstance value) in soundEffectInstances)
        {
            Stop(key);
        }
    }

    public void Resume(string key)
    {
        soundEffectInstances[key].Resume();
    }

    public void ResumeAll()
    {
        foreach ((string key, SoundEffectInstance value) in soundEffectInstances)
        {
            Resume(key);
        }
    }

    // Increment the volume of music
    public void IncMusicVolume()
    {
        if(MusicVol < MAGIC.SOUND.MAX_VOL)
        {
            MusicVol += MAGIC.SOUND.VOLUME_UNIT;
        }
    }

    // Decrement the volume of music
    public void DecMusicVolume()
    {
        if (MusicVol > MAGIC.SOUND.MIN_VOL)
        {
            MusicVol -= MAGIC.SOUND.VOLUME_UNIT;
        }
    }

    // Increment the volume of sound effects
    public void IncSfxVolume()
    {
        if(SfxVol < MAGIC.SOUND.MAX_VOL)
        {
            SfxVol += MAGIC.SOUND.VOLUME_UNIT;
        }
    }

    // Decrement the volume of sound effects
    public void DecSfxVolume()
    {
        if(SfxVol > MAGIC.SOUND.MIN_VOL)
        {
            SfxVol -= MAGIC.SOUND.VOLUME_UNIT;
        }
    }

}

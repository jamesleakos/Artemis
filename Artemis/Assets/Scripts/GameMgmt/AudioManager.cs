using UnityEngine;

[System.Serializable]
public class Sound
{

	public string name;
	public AudioClip clip;

    [Range(0f, 1f)]
    public float defaultVolume = 0.7f;
    [Range(0f, 1f)]
	public float volume = 0.7f;
	[Range(0.5f, 1.5f)]
	public float pitch = 1f;

	[Range(0f, 0.5f)]
	public float randomVolume = 0.1f;
	[Range(0f, 0.5f)]
	public float randomPitch = 0.1f;

	public bool loop = false;
    public enum SoundType { sfx, music }
    public SoundType soundType;

    private AudioSource source;

	public void SetSource(AudioSource _source)
	{
		source = _source;
		source.clip = clip;
		source.loop = loop;
	}

	public void Play()
	{
		source.volume = volume * (1 + Random.Range(-randomVolume / 2f, randomVolume / 2f));
		source.pitch = pitch * (1 + Random.Range(-randomPitch / 2f, randomPitch / 2f));
		source.Play();
	}

	public void Stop()
	{
		source.Stop();
	}

    public void ChangeVolume() {
        Debug.Log(source);
        source.volume = volume * (1 + Random.Range(-randomVolume / 2f, randomVolume / 2f));
    }

}

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;

    [Range(0f, 1f)]
    public float defaultMainVolume = 1f;
    [Range(0f, 1f)]
    public float mainVolume = 1f;

	[SerializeField]
	Sound[] sounds;

    public delegate void SFXSet(float setVolume, float mainVolume);
    public static event SFXSet OnSFXSet;

	void Awake()
	{
		if (instance != null)
		{
			if (instance != this)
			{
				Destroy(this.gameObject);
			}
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(this);
		}
	}

	void Start()
	{
		for (int i = 0; i < sounds.Length; i++)
		{
			GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
			_go.transform.SetParent(this.transform);
			sounds[i].SetSource(_go.AddComponent<AudioSource>());
		}

        SetMain(PlayerPrefs.GetFloat("MainVolume", 1f));
        SetMusic(PlayerPrefs.GetFloat("MusicVolume",1f));
        SetSFX(PlayerPrefs.GetFloat("SFXVolume",1f));

		PlaySound("Music");
	}

	public void PlaySound(string _name)
	{
		for (int i = 0; i < sounds.Length; i++)
		{
			if (sounds[i].name == _name)
			{
				sounds[i].Play();
				return;
			}
		}

		// no sound with _name
		Debug.LogWarning("AudioManager: Sound not found in list, " + _name);
	}

	public void StopSound(string _name)
	{
		for (int i = 0; i < sounds.Length; i++)
		{
			if (sounds[i].name == _name)
			{
				sounds[i].Stop();
				return;
			}
		}

		// no sound with _name
		Debug.LogWarning("AudioManager: Sound not found in list, " + _name);
	}

    public void ChangeVolume(string _name) {
        for (int i = 0; i < sounds.Length; i++) {
            if (sounds[i].name == _name) {
                sounds[i].ChangeVolume();
                return;
            }
        }

        // no sound with _name
    }

    public void SetMain(float setVolume) {
        PlayerPrefs.SetFloat("MainVolume", setVolume);

        mainVolume = defaultMainVolume * setVolume;
        SetMusic(PlayerPrefs.GetFloat("MusicVolume",1f));
        SetSFX(PlayerPrefs.GetFloat("SFXVolume",1f));
    }

    public void SetSFX(float setVolume) {
        PlayerPrefs.SetFloat("SFXVolume", setVolume);
        //OnSFXSet(setVolume, mainVolume);

        for (int i = 0; i < sounds.Length; i++) {
            if (sounds[i].soundType == Sound.SoundType.sfx) {
                sounds[i].volume = sounds[i].defaultVolume * setVolume * mainVolume;
            }
        }
    }

    public void SetMusic(float setVolume) {
        PlayerPrefs.SetFloat("MusicVolume", setVolume);

        for (int i = 0; i < sounds.Length; i++) {
            if (sounds[i].soundType == Sound.SoundType.music) {
                sounds[i].volume = sounds[i].defaultVolume * setVolume * mainVolume;
                sounds[i].ChangeVolume();
            }
        }
    }

}

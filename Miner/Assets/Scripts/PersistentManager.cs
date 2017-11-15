using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class PersistentManager : MonoBehaviour
{
	enum TRANSITION
	{
		None,
		Fade
	}
	[SerializeField]
	TRANSITION trans;
	public enum SAVEKEY
	{
		MusicVolume, SfxVolume, Level, MaxLevel,
		Total
	}
	public enum MUSIC_CLIP { None = -1, DayTime, NightTime, Start, Boss, Victory }
	static public MUSIC_CLIP currClip = MUSIC_CLIP.None;
	static public AudioClip[] music_clips;
	static public AudioClip[] audioAttacks, audioDeaths;
	static public AudioSource musicPlayer, sfxPlayer, transition;

	AsyncOperation aOp;
	static public GameObject loadingScreen;
	public GameObject backBtn;
	static public Transform canvas;

	static public PersistentManager mmScript;

	private void Awake()
	{
		if (mmScript)
		{
			Destroy(gameObject);
			return;
		}
		musicPlayer = GetComponents<AudioSource>()[0];
		sfxPlayer = GetComponents<AudioSource>()[1];
		transition = GetComponents<AudioSource>()[2];
		if (PlayerPrefs.HasKey(SAVEKEY.MusicVolume.ToString()))
		{
			musicPlayer.volume = transition.volume = PlayerPrefs.GetFloat(SAVEKEY.MusicVolume.ToString());
			SetAllAudioSources();
		}
		else
		{
			ResetKeys();
		}
		mmScript = GetComponent<PersistentManager>();
		DontDestroyOnLoad(gameObject);
		audioAttacks = Resources.LoadAll<AudioClip>("Attacks");
		audioDeaths = Resources.LoadAll<AudioClip>("Deaths");
		music_clips = Resources.LoadAll<AudioClip>("Music");
		musicPlayer.ignoreListenerVolume = true;
		transition.ignoreListenerVolume = true;
	}

	void LoadScreen(string _sceneName)
	{
		Time.timeScale = 1;
		if (canvas)
		{

		Transform menu = canvas.Find("Menus");
		if (menu)
			menu.gameObject.SetActive(false);
		if (trans == TRANSITION.Fade)
			menu.Find("Fader").GetComponent<RawImage>().CrossFadeAlpha(1f, 2f, true);
		}
		if (loadingScreen)
		{
			loadingScreen.SetActive(true);
			StartCoroutine(LoadLevelWithProgressBar(_sceneName));
		}
		else
			SceneManager.LoadSceneAsync(_sceneName);
	}

	static public void SetAllAudioSources()
	{
		AudioSource[] allSounds = FindObjectsOfType<AudioSource>();
		float sfxVol = PlayerPrefs.GetFloat(SAVEKEY.SfxVolume.ToString());
		foreach (AudioSource adiSource in allSounds)
		{
			if (adiSource != musicPlayer && adiSource != transition)
			{
				adiSource.volume = sfxVol;
			}
		}
	}

	public void SetSliderValues(Transform _parent)
	{
		Transform musicSliderT = _parent.Find("MusicSlider");
		if (musicSliderT)
		{
			Slider musicSlider = musicSliderT.GetComponent<Slider>();
			musicSlider.value = CurrMusicPlayer().volume;
			if (musicSlider.onValueChanged.GetPersistentEventCount() ==0)
			{
				musicSlider.onValueChanged.AddListener(SetMusicVol);
			}
		}
		Transform sfxSliderT = _parent.Find("SFXSlider");
		if (sfxSliderT)
		{
			Slider sfxSlider = sfxSliderT.GetComponent<Slider>();
			sfxSlider.value = sfxPlayer.volume;
			if (sfxSlider.onValueChanged.GetPersistentEventCount() == 0)
			{
				sfxSlider.onValueChanged.AddListener(SetSFXVol);
			}
		}
	}

	public void SetMusicVol(float _vol)
	{
		CurrMusicPlayer().volume = _vol;
		SetSaveKey(SAVEKEY.MusicVolume, _vol);
	}

	public void SetSFXVol(float _vol)
	{
		SetSaveKey(SAVEKEY.SfxVolume, _vol);
		SetAllAudioSources();
	}

	public void ChangeScene(string _sceneName)
	{
		if (_sceneName == "")
			ChangeScene();
		LoadScreen(_sceneName);
	}

	public void ChangeScene(int _level)
	{
		LoadScreen("Level" + _level);
	}

	public void ChangeScene()
	{
		LoadScreen("Level" + PlayerPrefs.GetFloat(SAVEKEY.Level.ToString()));
	}


	static public void ResetKeys()
	{
		SetSaveKey(SAVEKEY.Level, 0f);
		SetSaveKey(SAVEKEY.MaxLevel, 0f);
		SetSaveKey(SAVEKEY.MusicVolume, 1f);
		SetSaveKey(SAVEKEY.SfxVolume, 1f);
	}

	static public void SetSaveKey(SAVEKEY _saveKey, float _val)
	{
		PlayerPrefs.SetFloat(_saveKey.ToString(), _val);
	}

	public AudioSource CurrMusicPlayer()
	{
		return (musicPlayer.isPlaying ? transition : musicPlayer);
	}

	public void SwitchMusic(MUSIC_CLIP _newclip)
	{
		if (_newclip == currClip)
		{
			return;
		}
		currClip = _newclip;
		AudioSource temp = CurrMusicPlayer();
		temp.Stop();
		temp.PlayOneShot(music_clips[(uint)currClip]);
	}

	public void FadeMusic(MUSIC_CLIP _newclip)
	{
		if (_newclip == currClip)
		{
			return;
		}
		currClip = _newclip;
		if (musicPlayer.isPlaying)
		{
			transition.PlayOneShot(music_clips[(uint)_newclip]);
			StartCoroutine(FadeInAndOut(transition, musicPlayer));
		}
		else
		{
			musicPlayer.PlayOneShot(music_clips[(uint)_newclip]);
			StartCoroutine(FadeInAndOut(musicPlayer, transition));
		}
	}

	static public void PlaySoundEffect(AudioClip _clip)
	{
		sfxPlayer.PlayOneShot(_clip);
	}

	static public void PlayRandomAttack()
	{
		sfxPlayer.PlayOneShot(audioAttacks[UnityEngine.Random.Range(0, audioAttacks.Length)]);
	}

	static public void PlayRandomDeath()
	{
		sfxPlayer.PlayOneShot(audioDeaths[UnityEngine.Random.Range(0, audioDeaths.Length)]);
	}

	IEnumerator FadeInAndOut(AudioSource _in, AudioSource _out, float _speed = .003f)
	{
		float maxVolume = PlayerPrefs.GetFloat(SAVEKEY.MusicVolume.ToString());
		while (_in.volume < maxVolume)
		{
			_in.volume += _speed;
			_out.volume -= _speed;
			yield return null;
		}
		_in.volume = maxVolume;
		_out.volume = 0f;
		_out.Stop();
		yield break;
	}
	IEnumerator LoadLevelWithProgressBar(string _sceneName)
	{
		yield return new WaitForSeconds(1);//avoids loading conflicts

		aOp = SceneManager.LoadSceneAsync(_sceneName);

		aOp.allowSceneActivation = false;
		Slider loadingProgBar = null;
		if (loadingScreen)
			loadingProgBar = loadingScreen.transform.GetChild(0).GetComponent<Slider>();
		while (!aOp.isDone)
		{
			if (loadingProgBar)
				loadingProgBar.value = aOp.progress;

			if (aOp.progress == 0.9f)
			{
				if (loadingProgBar)
					loadingProgBar.value = 1.0f;
				aOp.allowSceneActivation = true;
			}

			yield return null;
		}

	}

}

/*
    This script was provided to us by Professor Eric May.
*/
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance { get; private set; }
  [SerializeField] AudioSource _musicSource, _sfxSource;
  [SerializeField] SettingsDataObject _settingsData;
  [SerializeField] AudioMixer _audioMixer;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
    _musicSource = transform.GetChild(0).GetComponent<AudioSource>();
    //_sfxSource = transform.GetChild(1).GetComponent<AudioSource>();
  }

  void Start()
  {
    // Load settings
    _settingsData.LoadSettings();
    // Apply settings to audio sources
    _musicSource.volume = _settingsData.MusicVolume;
    //_sfxSource.volume = _settingsData.SFXVolume;
    _audioMixer.SetFloat("MasterVolume", Mathf.Log10((_settingsData.MasterVolume + .0001f) / 100) * 20);
    _audioMixer.SetFloat("MusicVolume", Mathf.Log10((_settingsData.MusicVolume + .0001f) / 100) * 20);
    _audioMixer.SetFloat("SFXVolume", Mathf.Log10((_settingsData.SFXVolume + .0001f) / 100) * 20);
  }

  // Play a music track
  public void PlayMusic(AudioClip clip = null)
  {
    if (clip == null)
    {
      _musicSource.Play();
    }
    else if (_musicSource.clip != clip)
    {
      _musicSource.clip = clip;
      _musicSource.Play();
    }
  }
  
  // Update is called once per frame
  void Update()
  {

  }
}

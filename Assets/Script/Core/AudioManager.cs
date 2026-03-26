using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio; // Required for Mixer

public class AudioManager : PersistentSingleton<AudioManager> {
    [Header("Audio Mixer")] // --- NEW SECTION ---
    public AudioMixer mainMixer;
    // These strings must match the "Exposed Parameter" names in your Mixer
    private const string MIXER_MUSIC = "MusicVol";
    private const string MIXER_SFX = "SFXVol";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource playerSource;

    [Header("Clips Library")]
    public List<AudioClip> clipList;
    private Dictionary<string, AudioClip> clipDict = new Dictionary<string, AudioClip>();

    protected override void Initialize() {
        foreach (var clip in clipList) {
            if (!clipDict.ContainsKey(clip.name))
                clipDict.Add(clip.name, clip);
        }

        // --- NEW: LOAD SAVED VOLUMES ---
        // We wait 1 frame or call explicitly because Mixers sometimes fail to set in Awake
        LoadVolumeSettings();
    }

    void LoadVolumeSettings() {
        // Load saved values (Default to 0dB = Full Volume if not found)
        float musicVol = PlayerPrefs.GetFloat(MIXER_MUSIC, 0f);
        float sfxVol = PlayerPrefs.GetFloat(MIXER_SFX, 0f);

        mainMixer.SetFloat(MIXER_MUSIC, musicVol);
        mainMixer.SetFloat(MIXER_SFX, sfxVol);
    }

    // Call this from your UI Slider
    public void SetMusicVolume(float sliderValue) {
        // Sliders are usually 0 to 1. Mixers are -80 to 0.
        // Formula: Mathf.Log10(sliderValue) * 20
        float dbVolume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;

        mainMixer.SetFloat(MIXER_MUSIC, dbVolume);

        // Save it!
        PlayerPrefs.SetFloat(MIXER_MUSIC, dbVolume);
    }

    // Call this from your UI Slider
    public void SetSFXVolume(float sliderValue) {
        float dbVolume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;

        mainMixer.SetFloat(MIXER_SFX, dbVolume);

        // Save it!
        PlayerPrefs.SetFloat(MIXER_SFX, dbVolume);
    }

    // ... (Keep your PlaySFX, PlayPlayerSound, PlayMusic functions same as before) ...
    // 1. General SFX (UI, Doors)
    public void PlaySFX(string clipName, float volume = 1f) {
        if (clipDict.TryGetValue(clipName, out AudioClip clip)) {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // 2. Player Specific Sound (Uses the new source)
    public void PlayPlayerSound(string clipName, float volume = 1f) {
        if (clipDict.TryGetValue(clipName, out AudioClip clip)) {
            playerSource.pitch = Random.Range(0.9f, 1.1f);
            playerSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(string clipName) {
        if (clipDict.TryGetValue(clipName, out AudioClip clip)) {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void StopMusic() {
        if (musicSource != null) {
            musicSource.Stop();
        }
    }
}
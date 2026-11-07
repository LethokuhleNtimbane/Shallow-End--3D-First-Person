
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Start()
    {
        LoadVolume();

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (SFXSlider != null)
        {
            SFXSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        myMixer.SetFloat(
            "Music",
            volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f
        );

        PlayerPrefs.SetFloat(MUSIC_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        myMixer.SetFloat(
            "SFX",
            volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f
        );

        PlayerPrefs.SetFloat(SFX_KEY, volume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVolume);
            SetMusicVolume(musicVolume);
        }

        if (SFXSlider != null)
        {
            SFXSlider.SetValueWithoutNotify(sfxVolume);
            SetSFXVolume(sfxVolume);
        }
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (SFXSlider != null)
            SFXSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}

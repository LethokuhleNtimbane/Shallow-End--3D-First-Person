
using UnityEngine;
using UnityEngine.UI;

public class AudioManagerSettings : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.instance;

        if (audioManager == null)
        {
            Debug.LogError("AudioManager was not found!");
            return;
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                audioManager.GetMusicVolume()
            );

            musicSlider.onValueChanged.AddListener(
                audioManager.SetMusicVolume
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(
                audioManager.GetSFXVolume()
            );

            sfxSlider.onValueChanged.AddListener(
                audioManager.SetSFXVolume
            );
        }
    }

    private void OnDestroy()
    {
        if (audioManager == null)
            return;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(
                audioManager.SetMusicVolume
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(
                audioManager.SetSFXVolume
            );
        }
    }
}

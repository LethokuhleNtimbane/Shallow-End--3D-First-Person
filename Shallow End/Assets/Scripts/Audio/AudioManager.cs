using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip backgroundDaytime;
    public AudioClip backgroundNightTime;
    public AudioClip Dash;
    public AudioClip FlintLight;
    public AudioClip Monster;
    public AudioClip LowHealth;
    public AudioClip IntoWater;
    public AudioClip NightTimeFire;
    public AudioClip PickupItem;
    public AudioClip OpenCraftingBench;

    public void Start()
    {
        musicSource.clip = backgroundNightTime;
        musicSource.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}

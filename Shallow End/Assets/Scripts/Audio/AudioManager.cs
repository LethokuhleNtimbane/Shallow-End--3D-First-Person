using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Music")]
    public AudioClip backgroundDaytime;
    public AudioClip backgroundNightTime;

    [Header("Music Fade")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float musicVolume = 1f;

    [Header("SFX")]
    public AudioClip Dash;
    public AudioClip FlintLight;
    public AudioClip Monster;
    public AudioClip LowHealth;
    public AudioClip IntoWater;
    public AudioClip NightTimeFire;
    public AudioClip PickupItem;
    public AudioClip OpenCraftingBench;

    private bool isNight = false;
    private bool musicStarted = false;
    private Coroutine musicFadeCoroutine;

    private void Update()
    {

        if (TimeController.instance == null)
            return;

        float currentHour = TimeController.instance.CurrentHour;


        bool shouldBeNight = currentHour >= 21f || currentHour < 6f;


        if (!musicStarted)
        {
            isNight = shouldBeNight;
            musicStarted = true;

            if (shouldBeNight)
            {
                StartMusic(backgroundNightTime);
            }
            else
            {
                StartMusic(backgroundDaytime);
            }

            return;
        }

        if (shouldBeNight != isNight)
        {
            isNight = shouldBeNight;

            if (shouldBeNight)
            {
                ChangeMusic(backgroundNightTime);
            }
            else
            {
                ChangeMusic(backgroundDaytime);
            }
        }
    }

    private void StartMusic(AudioClip newClip)
    {
        if (newClip == null)
            return;

        musicSource.clip = newClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void ChangeMusic(AudioClip newClip)
    {
        if (newClip == null)
            return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(FadeMusic(newClip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;


        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            musicSource.volume = Mathf.Lerp(
                startVolume,
                0f,
                t
            );

            yield return null;
        }

        musicSource.volume = 0f;

     
        musicSource.clip = newClip;
        musicSource.Play();


        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            musicSource.volume = Mathf.Lerp(
                0f,
                musicVolume,
                t
            );

            yield return null;
        }

        musicSource.volume = musicVolume;

        musicFadeCoroutine = null;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            SFXSource.PlayOneShot(clip);
        }
    }
}



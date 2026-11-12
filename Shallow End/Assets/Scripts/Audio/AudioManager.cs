using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Background Music")]
    public AudioClip backgroundDaytime;
    public AudioClip backgroundNightTime;

    [Header("Main Menu Intro")]
    [SerializeField] private AudioClip mainMenuIntro;

    [Header("Opening Animation")]
    [SerializeField] private Animator openingAnimator;
    [SerializeField] private string openingAnimationTrigger = "PlayOpeningAnimation";

    [Header("Audio Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;

    [Header("Sound Effects")]
    public AudioClip Dash;
    public AudioClip FlintLight;
    public AudioClip Monster;
    public AudioClip LowHealth;
    public AudioClip IntoWater;
    public AudioClip NightTimeFire;
    public AudioClip PickupItem;
    public AudioClip OpenCraftingBench;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string MENU_SCENE = "MainMenuScene";

    private Coroutine musicFadeCoroutine;
    private Coroutine introCoroutine;

    private float musicFadeMultiplier = 1f;
    private AudioClip targetMusicClip;

    private bool introPlayed;
    private bool introPlaying;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        ApplyMusicVolume();
        ApplySFXVolume();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == MENU_SCENE)
        {
            // Play the opening sequence only once per game launch
            if (!introPlayed)
            {
                StartMenuIntro();
                return;
            }

            // Do not start normal menu music during the opening sequence
            if (introPlaying)
                return;

            PlayMusicIfNeeded(backgroundDaytime);
            return;
        }

        // Normal game music
        PlayMusicIfNeeded(GetCurrentGameMusic());
    }

    private AudioClip GetCurrentGameMusic()
    {
        if (TimeController.instance != null)
        {
            float currentHour = TimeController.instance.CurrentHour;

            if (currentHour >= 21f || currentHour < 6f)
                return backgroundNightTime;
        }

        return backgroundDaytime;
    }

    private void StartMenuIntro()
    {
        if (introPlaying || introPlayed)
            return;

        introPlayed = true;
        introPlaying = true;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        targetMusicClip = backgroundDaytime;

        // Stop normal background music
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        introCoroutine = StartCoroutine(PlayMenuIntro());
    }

    private IEnumerator PlayMenuIntro()
    {
        // Play the opening song
        if (mainMenuIntro != null && SFXSource != null)
        {
            SFXSource.Stop();

            SFXSource.clip = mainMenuIntro;
            SFXSource.volume = sfxVolume;
            SFXSource.loop = false;

            SFXSource.Play();

            // Wait for the exact length of the audio clip.
            //
            // Your clip is 10.030 seconds, so this waits approximately:
            // 00:10:030
            //
            // The animation trigger happens immediately after this wait.
            yield return new WaitForSecondsRealtime(mainMenuIntro.length);
        }

        // Opening song has finished.
        // Play the animation immediately.
        if (openingAnimator != null)
        {
            openingAnimator.SetTrigger(openingAnimationTrigger);
        }

        introPlaying = false;
        introCoroutine = null;

        // Start normal menu music
        PlayMusicIfNeeded(backgroundDaytime);
    }

    private void PlayMusicIfNeeded(AudioClip newClip)
    {
        if (newClip == null || musicSource == null || introPlaying)
            return;

        if (targetMusicClip == newClip)
        {
            if (musicFadeCoroutine != null)
                return;

            if (musicSource.clip == newClip && musicSource.isPlaying)
                return;
        }

        targetMusicClip = newClip;

        if (!musicSource.isPlaying || musicSource.clip == null)
            StartMusic(newClip);
        else
            ChangeMusic(newClip);
    }

    private void StartMusic(AudioClip newClip)
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicSource.clip = newClip;
        musicFadeMultiplier = 1f;

        ApplyMusicVolume();
        musicSource.Play();
    }

    private void ChangeMusic(AudioClip newClip)
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeMusic(newClip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        float timer = 0f;
        float startMultiplier = musicFadeMultiplier;

        // Fade out
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = fadeDuration > 0f
                ? Mathf.Clamp01(timer / fadeDuration)
                : 1f;

            musicFadeMultiplier = Mathf.Lerp(startMultiplier, 0f, t);
            ApplyMusicVolume();

            yield return null;
        }

        musicFadeMultiplier = 0f;
        ApplyMusicVolume();

        musicSource.clip = newClip;
        musicSource.Play();

        timer = 0f;

        // Fade in
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = fadeDuration > 0f
                ? Mathf.Clamp01(timer / fadeDuration)
                : 1f;

            musicFadeMultiplier = Mathf.Lerp(0f, 1f, t);
            ApplyMusicVolume();

            yield return null;
        }

        musicFadeMultiplier = 1f;
        ApplyMusicVolume();

        musicFadeCoroutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(MUSIC_KEY, musicVolume);
        PlayerPrefs.Save();

        ApplyMusicVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(SFX_KEY, sfxVolume);
        PlayerPrefs.Save();

        ApplySFXVolume();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * musicFadeMultiplier;
    }

    private void ApplySFXVolume()
    {
        if (SFXSource != null)
            SFXSource.volume = sfxVolume;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && SFXSource != null)
            SFXSource.PlayOneShot(clip);
    }
}
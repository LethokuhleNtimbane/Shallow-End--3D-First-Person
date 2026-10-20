
using System;
using UnityEngine;

public class RainWeatherController : MonoBehaviour
{
    private enum WeatherState
    {
        Waiting,
        Approaching,
        Raining,
        Clearing
    }

    [Header("References")]
    [SerializeField] private TimeController timeController;
    [SerializeField] private Light directionalLight;
    [SerializeField] private Material cloudMaterial;
    [SerializeField] private ParticleSystem rainParticleSystem;

    [Header("Cloud Material")]
    [SerializeField] private string cloudsAlphaProperty = "_Clouds_Alpha";
    [SerializeField] private float stormCloudAlpha = 30f;

    [Header("Storm Lighting")]
    [SerializeField]
    private Color stormLightColour =
        new Color(0.45f, 0.48f, 0.55f, 1f);

    [Header("Transition Timing - In-Game Hours")]
    [SerializeField, Min(0.01f)]
    private float minimumTransitionHours = 0.25f;

    [SerializeField, Range(0.01f, 1f)]
    private float maximumTransitionHours = 1f;

    [Header("Rain Duration")]
    [SerializeField, Min(0.01f)]
    private float rainDurationHours = 2f;

    [Header("Random Weather Schedule")]
    [SerializeField, Min(1f)]
    private float initialMinimumWaitHours = 24f;

    [SerializeField, Min(1f)]
    private float initialMaximumWaitHours = 96f;

    [SerializeField, Min(24f)]
    private float minimumCooldownAfterStormHours = 24f;

    [SerializeField, Min(0f)]
    private float additionalMinimumWaitHours = 24f;

    [SerializeField, Min(0f)]
    private float additionalMaximumWaitHours = 72f;

    [Header("Testing")]
    [SerializeField] private bool showDebugMessages = true;

    private WeatherState currentState = WeatherState.Waiting;

    private Color originalLightColour;
    private float originalCloudAlpha;

    private bool originalLightColourCaptured;
    private bool originalCloudAlphaCaptured;
    private bool initialized;

    private DateTime gameStartTime;
    private DateTime nextStormTime;
    private DateTime transitionStartTime;
    private DateTime rainStartTime;

    private float currentTransitionDurationHours;

    private Color transitionStartLightColour;
    private float transitionStartCloudAlpha;

    private bool firstDayCompleted;

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        if (!initialized)
        {
            TryInitialize();
            return;
        }

        if (timeController == null)
            return;

        DateTime gameTime = timeController.CurrentTime;

        switch (currentState)
        {
            case WeatherState.Waiting:
                CheckForStorm(gameTime);
                break;

            case WeatherState.Approaching:
                UpdateStormApproach(gameTime);
                break;

            case WeatherState.Raining:
                UpdateRain(gameTime);
                break;

            case WeatherState.Clearing:
                UpdateWeatherRecovery(gameTime);
                break;
        }
    }

    private void TryInitialize()
    {
        if (initialized)
            return;

        if (timeController == null)
            timeController = TimeController.instance;

        if (timeController == null)
            return;

        if (directionalLight == null)
        {
            Debug.LogWarning(
                "RainWeatherController: Assign the directional light.");

            enabled = false;
            return;
        }

        if (cloudMaterial == null)
        {
            Debug.LogWarning(
                "RainWeatherController: Assign the cloud material.");

            enabled = false;
            return;
        }

        if (!cloudMaterial.HasProperty(cloudsAlphaProperty))
        {
            Debug.LogWarning(
                "RainWeatherController: The assigned material does not contain "
                + cloudsAlphaProperty);

            enabled = false;
            return;
        }

        // Remember the original weather settings.
        originalLightColour = directionalLight.color;
        originalLightColourCaptured = true;

        originalCloudAlpha =
            cloudMaterial.GetFloat(cloudsAlphaProperty);

        originalCloudAlphaCaptured = true;

        // Ensure rain does not play automatically.
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Record the starting game time.
        gameStartTime = timeController.CurrentTime;
        firstDayCompleted = false;

        // No storm can start during the first 24 in-game hours.
        nextStormTime = gameStartTime.AddHours(24f);

        initialized = true;

        Log("Weather initialized. Storms are disabled during the first day.");
    }

    private void CheckForStorm(DateTime gameTime)
    {
        // Protect the first 24 in-game hours.
        if (!firstDayCompleted)
        {
            if (gameTime < gameStartTime.AddHours(24f))
                return;

            firstDayCompleted = true;

            // Once the first day is complete, begin a random waiting period.
            float minimumWait =
                Mathf.Max(1f, initialMinimumWaitHours);

            float maximumWait =
                Mathf.Max(minimumWait, initialMaximumWaitHours);

            float randomWait =
                UnityEngine.Random.Range(minimumWait, maximumWait);

            nextStormTime = gameTime.AddHours(randomWait);

            Log(
                "First day completed. A storm may occur after a random wait of "
                + randomWait.ToString("F1")
                + " in-game hours.");

            return;
        }

        if (gameTime >= nextStormTime)
        {
            BeginStormApproach(gameTime);
        }
    }

    private void BeginStormApproach(DateTime gameTime)
    {
        currentState = WeatherState.Approaching;

        transitionStartTime = gameTime;
        transitionStartLightColour = directionalLight.color;

        transitionStartCloudAlpha =
            cloudMaterial.GetFloat(cloudsAlphaProperty);

        float minimum =
            Mathf.Clamp(minimumTransitionHours, 0.01f, 1f);

        float maximum =
            Mathf.Clamp(maximumTransitionHours, minimum, 1f);

        currentTransitionDurationHours =
            UnityEngine.Random.Range(minimum, maximum);

        Log(
            "Storm approaching. Transition duration: "
            + currentTransitionDurationHours.ToString("F2")
            + " in-game hours.");
    }

    private void UpdateStormApproach(DateTime gameTime)
    {
        float elapsedHours =
            (float)(gameTime - transitionStartTime).TotalHours;

        float progress = currentTransitionDurationHours > 0f
            ? Mathf.Clamp01(
                elapsedHours / currentTransitionDurationHours)
            : 1f;

        float smoothProgress =
            Mathf.SmoothStep(0f, 1f, progress);

        // Gradually darken the directional light.
        directionalLight.color = Color.Lerp(
            transitionStartLightColour,
            stormLightColour,
            smoothProgress);

        // Gradually increase cloud alpha.
        float newCloudAlpha = Mathf.Lerp(
            transitionStartCloudAlpha,
            stormCloudAlpha,
            smoothProgress);

        cloudMaterial.SetFloat(
            cloudsAlphaProperty,
            newCloudAlpha);

        // Start rain only after both transitions finish.
        if (progress >= 1f)
        {
            directionalLight.color = stormLightColour;

            cloudMaterial.SetFloat(
                cloudsAlphaProperty,
                stormCloudAlpha);

            BeginRain(gameTime);
        }
    }

    private void BeginRain(DateTime gameTime)
    {
        currentState = WeatherState.Raining;
        rainStartTime = gameTime;

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Play(true);
        }
        else
        {
            Debug.LogWarning(
                "RainWeatherController: No rain Particle System assigned.");
        }

        Log(
            "Rain started. It will last "
            + rainDurationHours.ToString("F1")
            + " in-game hours.");
    }

    private void UpdateRain(DateTime gameTime)
    {
        float elapsedHours =
            (float)(gameTime - rainStartTime).TotalHours;

        if (elapsedHours < rainDurationHours)
            return;

        EndRain(gameTime);
    }

    private void EndRain(DateTime gameTime)
    {
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting);
        }

        currentState = WeatherState.Clearing;

        transitionStartTime = gameTime;
        transitionStartLightColour = directionalLight.color;

        transitionStartCloudAlpha =
            cloudMaterial.GetFloat(cloudsAlphaProperty);

        float minimum =
            Mathf.Clamp(minimumTransitionHours, 0.01f, 1f);

        float maximum =
            Mathf.Clamp(maximumTransitionHours, minimum, 1f);

        currentTransitionDurationHours =
            UnityEngine.Random.Range(minimum, maximum);

        Log("Rain ended. Weather is returning to normal.");
    }

    private void UpdateWeatherRecovery(DateTime gameTime)
    {
        float elapsedHours =
            (float)(gameTime - transitionStartTime).TotalHours;

        float progress = currentTransitionDurationHours > 0f
            ? Mathf.Clamp01(
                elapsedHours / currentTransitionDurationHours)
            : 1f;

        float smoothProgress =
            Mathf.SmoothStep(0f, 1f, progress);

        // Restore the original light colour.
        directionalLight.color = Color.Lerp(
            transitionStartLightColour,
            originalLightColour,
            smoothProgress);

        // Restore the original cloud alpha.
        cloudMaterial.SetFloat(
            cloudsAlphaProperty,
            Mathf.Lerp(
                transitionStartCloudAlpha,
                originalCloudAlpha,
                smoothProgress));

        if (progress >= 1f)
        {
            directionalLight.color = originalLightColour;

            cloudMaterial.SetFloat(
                cloudsAlphaProperty,
                originalCloudAlpha);

            ScheduleNextStorm(gameTime);

            currentState = WeatherState.Waiting;
        }
    }

    private void ScheduleNextStorm(DateTime gameTime)
    {
        // Guarantee at least 24 in-game hours between storms.
        float minimumCooldown =
            Mathf.Max(24f, minimumCooldownAfterStormHours);

        float minimumExtra =
            Mathf.Max(0f, additionalMinimumWaitHours);

        float maximumExtra =
            Mathf.Max(minimumExtra, additionalMaximumWaitHours);

        float randomExtraWait =
            UnityEngine.Random.Range(minimumExtra, maximumExtra);

        float totalWait =
            minimumCooldown + randomExtraWait;

        nextStormTime = gameTime.AddHours(totalWait);

        Log(
            "Weather cleared. Next storm scheduled after "
            + totalWait.ToString("F1")
            + " in-game hours. Earliest next storm: "
            + nextStormTime.ToString("yyyy-MM-dd HH:mm"));
    }

    private void Log(string message)
    {
        if (showDebugMessages)
        {
            Debug.Log("[Rain Weather] " + message);
        }
    }

    private void OnDisable()
    {
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void OnDestroy()
    {
        // Restore the original settings when this controller is destroyed.
        if (originalLightColourCaptured && directionalLight != null)
        {
            directionalLight.color = originalLightColour;
        }

        if (originalCloudAlphaCaptured &&
            cloudMaterial != null &&
            cloudMaterial.HasProperty(cloudsAlphaProperty))
        {
            cloudMaterial.SetFloat(
                cloudsAlphaProperty,
                originalCloudAlpha);
        }
    }
}

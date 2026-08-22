using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController instance;
    public event Action OnHourChanged;
    [SerializeField]private float TimeMultiplier;
    [SerializeField] private float StartHour;

    [SerializeField] private TextMeshProUGUI TimerText;

    [SerializeField] private Light sunLight;

    [SerializeField] private float sunRiseHour;

    [SerializeField] private float sunSetHour;

    [SerializeField] private Color dayAmbientLight;

    [SerializeField] private Color nightAmbientLight;

    [SerializeField] private AnimationCurve lightChangeCurve;

    [SerializeField] private float MaxSunLightIntensity;

    [SerializeField] private Light moonLight;

    [SerializeField] private TextMeshProUGUI daytext;

    [SerializeField] private float maxMonnLightIntensity;

    private int Day = 1;

    private int previousHour;
    private int previousMinute;

    private TimeSpan sunriseTime;

    private TimeSpan sunSetTime;
    private DateTime currentTime;

    public DateTime CurrentTime => currentTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(StartHour);

        sunriseTime = TimeSpan.FromHours(sunRiseHour);
        sunSetTime = TimeSpan.FromHours(sunSetHour);

        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;
    }
    public bool IsNight
    {
        get
        {
            return currentTime.TimeOfDay >= TimeSpan.FromHours(21) ||
                   currentTime.TimeOfDay < TimeSpan.FromHours(6);
        }
    }

    public bool CanSleep
    {
        get
        {
            return currentTime.TimeOfDay >= TimeSpan.FromHours(21);
        }
    }

    public void WakeUp()
    {
        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;

        Day++;

        if (daytext != null)
        {
            daytext.text = "Day: " + Day;
        }
    }
    // Update is called once per frame
    void Update()
    {
        UpdateTimeOfDay();
        RotateSun();
        UpdateLightSetting();
      
    }
    public void SetTimeMultiplier(float multiplier)
    {
        TimeMultiplier = multiplier;
    }
    private void DayCount()
    {
        if (currentTime.Hour == 0 && previousHour == 23)
        {
            Day++;
        }

        if (daytext != null)
        {
            daytext.text = "Day: " + Day;
        }
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * TimeMultiplier);

        if (TimerText != null)
        {
            TimerText.text = "Hour: " + currentTime.ToString("HH");
        }

        if (currentTime.Hour != previousHour || currentTime.Minute != previousMinute)
        {
            previousHour = currentTime.Hour;
            previousMinute = currentTime.Minute;

            OnHourChanged?.Invoke();
            DayCount();
        }
    }
    private void RotateSun()
    {
        float sunlightRotation;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunSetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunSetTime);
            TimeSpan timeSinceSunRise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunRise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunlightRotation = Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunRiseDuration = CalculateTimeDifference(sunSetTime, sunriseTime);
            TimeSpan timeSinceSunSet = CalculateTimeDifference(sunSetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunSet.TotalMinutes / sunsetToSunRiseDuration.TotalMinutes;

            sunlightRotation = Mathf.Lerp(180, 360 , (float)percentage);
        }
        sunLight.transform.rotation = Quaternion.AngleAxis(sunlightRotation, Vector3.right);
    }
    private void UpdateLightSetting()
    {
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        sunLight.intensity = Mathf.Lerp(0, MaxSunLightIntensity, lightChangeCurve.Evaluate(dotProduct));
        moonLight.intensity = Mathf.Lerp(maxMonnLightIntensity, 0, lightChangeCurve.Evaluate(dotProduct));
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(dotProduct));
    }
    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;
        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);

        }
        return difference;
    }
    
}

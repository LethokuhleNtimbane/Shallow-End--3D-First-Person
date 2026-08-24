using System;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController instance;

    public event Action OnHourChanged;


    [SerializeField] private float TimeMultiplier = 60f;
    [SerializeField] private float StartHour = 6f;


    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private TextMeshProUGUI daytext;


    [SerializeField] private Light sunLight;
    [SerializeField] private float sunRiseHour = 6f;
    [SerializeField] private float sunSetHour = 18f;

  
    [SerializeField] private Color dayAmbientLight;
    [SerializeField] private Color nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve;
    [SerializeField] private float MaxSunLightIntensity;


    [SerializeField] private Light moonLight;
    [SerializeField] private float maxMonnLightIntensity;

    private int previousHour;
    private int previousMinute;

    private TimeSpan sunriseTime;
    private TimeSpan sunSetTime;

    private DateTime currentTime;

    public DateTime CurrentTime => currentTime;


  

    void Start()
    {
        instance = this;

       
        currentTime = new DateTime(2000, 1, 1).AddHours(StartHour);

        sunriseTime = TimeSpan.FromHours(sunRiseHour);
        sunSetTime = TimeSpan.FromHours(sunSetHour);

        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;

        UpdateDayText();
        UpdateTimeText();
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
       

        currentTime = currentTime.Date.AddDays(1).AddHours(6);

        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;

        UpdateDayText();
        UpdateTimeText();
    }



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

    
    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(
            Time.deltaTime * TimeMultiplier
        );

        UpdateTimeText();

        if (currentTime.Hour != previousHour ||
            currentTime.Minute != previousMinute)
        {
            previousHour = currentTime.Hour;
            previousMinute = currentTime.Minute;

            OnHourChanged?.Invoke();

            UpdateDayText();
        }
    }



    private void UpdateTimeText()
    {
        if (TimerText != null)
        {
            TimerText.text = "Hour: " + currentTime.ToString("HH");
        }
    }



    private void UpdateDayText()
    {
        if (daytext != null)
        {
       

            int gameDay = (currentTime.Date - new DateTime(2000, 1, 1)).Days + 1;

            daytext.text = "Day: " + gameDay;
        }
    }

  

    private void RotateSun()
    {
        float sunlightRotation;

        if (currentTime.TimeOfDay > sunriseTime &&
            currentTime.TimeOfDay < sunSetTime)
        {
            TimeSpan sunriseToSunsetDuration =
                CalculateTimeDifference(sunriseTime, sunSetTime);

            TimeSpan timeSinceSunRise =
                CalculateTimeDifference(
                    sunriseTime,
                    currentTime.TimeOfDay
                );

            double percentage =
                timeSinceSunRise.TotalMinutes /
                sunriseToSunsetDuration.TotalMinutes;

            sunlightRotation =
                Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunRiseDuration =
                CalculateTimeDifference(
                    sunSetTime,
                    sunriseTime
                );

            TimeSpan timeSinceSunSet =
                CalculateTimeDifference(
                    sunSetTime,
                    currentTime.TimeOfDay
                );

            double percentage =
                timeSinceSunSet.TotalMinutes /
                sunsetToSunRiseDuration.TotalMinutes;

            sunlightRotation =
                Mathf.Lerp(180, 360, (float)percentage);
        }

        sunLight.transform.rotation =
            Quaternion.AngleAxis(
                sunlightRotation,
                Vector3.right
            );
    }


    private void UpdateLightSetting()
    {
        float dotProduct =
            Vector3.Dot(
                sunLight.transform.forward,
                Vector3.down
            );

        float curveValue =
            lightChangeCurve.Evaluate(dotProduct);

        sunLight.intensity =
            Mathf.Lerp(
                0,
                MaxSunLightIntensity,
                curveValue
            );

        moonLight.intensity =
            Mathf.Lerp(
                maxMonnLightIntensity,
                0,
                curveValue
            );

        RenderSettings.ambientLight =
            Color.Lerp(
                nightAmbientLight,
                dayAmbientLight,
                curveValue
            );
    }



    private TimeSpan CalculateTimeDifference(
        TimeSpan fromTime,
        TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }

        return difference;
    }
}
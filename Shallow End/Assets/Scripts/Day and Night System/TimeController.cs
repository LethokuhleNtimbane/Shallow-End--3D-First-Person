using System;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController instance;

    public event Action OnHourChanged;
    [SerializeField] private float TimeMultiplier = 60f;
    [SerializeField] private float StartHour = 6f;

    [SerializeField] private float sunriseHour = 6f;
    [SerializeField] private float sunsetHour = 21f;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private TextMeshProUGUI daytext;
    [SerializeField] private Light sunLight;

    [SerializeField] private float sunriseRotation = 0f;

    [SerializeField] private float sunsetRotation = 180f;

    [SerializeField] private float sunRotationOffset = 0f;

    [SerializeField] private float sunsetStartHour = 18f;
    [SerializeField] private float sunsetEndHour = 21f;

    [SerializeField] private Color dayAmbientLight = Color.white;
    [SerializeField] private Color nightAmbientLight = Color.black;

    [SerializeField] private float MaxSunLightIntensity = 1f;


    [SerializeField] private float horizonFadeAngle = 20f;

    [SerializeField] private float maxMoonLightIntensity = 0.5f;
    [SerializeField] private Material skyboxMaterial;


    [SerializeField] private string dayNightSliderProperty = "_DayNight_Slider";




    [SerializeField] private string nightBlendProperty = "_Night_Blend";

    [SerializeField] private Color daySkyColour;


    [SerializeField] private Color sunsetSkyColour;


    [SerializeField] private Color nightSkyColour;




    [SerializeField] private Color dayHorizonColour;


    [SerializeField] private Color sunsetHorizonColour;


    [SerializeField] private Color nightHorizonColour;



    [SerializeField] private Color dayGroundColour;


    [SerializeField] private Color sunsetGroundColour;


    [SerializeField] private Color nightGroundColour;






    [SerializeField] private string skyColourProperty = "_Sky_Colour";

    [SerializeField] private string nightSkyColourProperty = "_Night_Sky";

    [SerializeField] private string horizonColourProperty = "_Horizon_colour";

    [SerializeField] private string groundColourProperty = "_Ground_Colour";

    [SerializeField]
    private float sunsetColourPeakHour;





    [SerializeField]
    private float sunriseColourStartHour;



    [SerializeField] private float dayStarPower = 100f;
    [SerializeField] private float nightStarPower = 5f;

    [SerializeField] private float starsFadeInStartHour = 18f;
    [SerializeField] private float starsFadeInEndHour = 21f;

    [SerializeField] private float starsFadeOutStartHour = 5f;
    [SerializeField] private float starsFadeOutEndHour = 8f;

    [SerializeField]
    private string starPowerProperty =
        "_Star_Power";

    private DateTime currentTime;

    private int previousHour;
    private int previousMinute;

    private Material skyboxMaterialInstance;
    private Material cloudMaterialInstance;


    public DateTime CurrentTime => currentTime;

    public float CurrentHour =>
        (float)currentTime.TimeOfDay.TotalHours;



    public bool IsNight
    {
        get
        {
            return GetSunBrightness() <= 0f;
        }
    }


    public bool CanSleep
    {
        get
        {
            return IsNight;
        }
    }

    private void Start()
    {
        instance = this;


        currentTime =
            new DateTime(2000, 1, 1)
            .AddHours(StartHour);


        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;




        if (skyboxMaterial != null)
        {
            skyboxMaterialInstance =
                new Material(skyboxMaterial);

            RenderSettings.skybox =
                skyboxMaterialInstance;
        }






        UpdateDayText();
        UpdateTimeText();

        UpdateSun();
        UpdateLighting();
        UpdateSkybox();
        
    }



    private void Update()
    {
        UpdateTime();

        UpdateSun();

        UpdateLighting();

        UpdateSkybox();

 
    }



    private void UpdateTime()
    {
        currentTime =
            currentTime.AddSeconds(
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




    public void SetTimeMultiplier(float multiplier)
    {
        TimeMultiplier = multiplier;
    }




    public void AdvanceSleepTime(float hours)
    {
        currentTime =
            currentTime.AddHours(hours);

        UpdateTimeText();
        UpdateDayText();

        UpdateSun();
        UpdateLighting();
        UpdateSkybox();
  
    }



    public void SetTime(float hour)
    {
        hour =
            Mathf.Clamp(
                hour,
                0f,
                23.99f
            );


        int hours =
            Mathf.FloorToInt(hour);


        int minutes =
            Mathf.FloorToInt(
                (hour - hours) * 60f
            );


        currentTime =
            currentTime.Date
            .AddHours(hours)
            .AddMinutes(minutes);


        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;


        UpdateTimeText();
        UpdateDayText();

        UpdateSun();
        UpdateLighting();
        UpdateSkybox();

        OnHourChanged?.Invoke();
    }



    public void WakeUp()
    {
        currentTime =
            currentTime.Date
            .AddDays(1)
            .AddHours(sunriseHour);


        previousHour = currentTime.Hour;
        previousMinute = currentTime.Minute;


        UpdateDayText();
        UpdateTimeText();

        UpdateSun();
        UpdateLighting();
        UpdateSkybox();


        OnHourChanged?.Invoke();
    }




    private void UpdateTimeText()
    {
        if (TimerText != null)
        {
            TimerText.text =
                "Hour: " +
                currentTime.ToString("HH");
        }
    }


    private void UpdateDayText()
    {
        if (daytext != null)
        {
            int gameDay =
                (currentTime.Date -
                 new DateTime(2000, 1, 1)).Days + 1;


            daytext.text =
                "Day: " + gameDay;
        }
    }




    private void UpdateSun()
    {
        if (sunLight == null)
            return;


        float hour = CurrentHour;

        float rotation;



        if (hour >= sunriseHour &&
            hour < sunsetHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunriseHour,
                    sunsetHour,
                    hour
                );


            rotation =
                Mathf.Lerp(
                    sunriseRotation,
                    sunsetRotation,
                    t
                );
        }



        else if (hour >= sunsetHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunsetHour,
                    24f,
                    hour
                );


            rotation =
                Mathf.Lerp(
                    sunsetRotation,
                    sunriseRotation + 360f,
                    t
                );
        }




        else
        {
            float t =
                Mathf.InverseLerp(
                    0f,
                    sunriseHour,
                    hour
                );


            rotation =
                Mathf.Lerp(
                    sunsetRotation,
                    sunriseRotation + 360f,
                    t
                );
        }


        rotation += sunRotationOffset;


        sunLight.transform.localRotation =
            Quaternion.Euler(
                rotation,
                0f,
                0f
            );
    }



    private float GetSunBrightness()
    {
        if (sunLight == null)
            return 0f;


        float sunHeight =
            Vector3.Dot(
                -sunLight.transform.forward,
                Vector3.up
            );


        float brightness =
            Mathf.InverseLerp(
                -0.05f,
                Mathf.Sin(
                    horizonFadeAngle *
                    Mathf.Deg2Rad
                ),
                sunHeight
            );


        return Mathf.Clamp01(brightness);
    }



    private void UpdateLighting()
    {
        float sunBrightness =
            GetSunBrightness();




        if (sunLight != null)
        {
            sunLight.intensity =
                MaxSunLightIntensity *
                sunBrightness;
        }
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight,sunBrightness);
    }




    private float GetDayNightSlider()
    {
        return CurrentHour / 24f;
    }

    private float GetNightBlend()
    {
        float hour = CurrentHour;
        if (hour >= sunriseHour &&
            hour < sunsetStartHour)
        {
            return 0f;
        }
        if (hour >= sunsetStartHour &&
            hour < sunsetEndHour)
        {
            return 0f;
        }
        if (hour >= sunsetEndHour ||
            hour < sunriseColourStartHour)
        {
            return 1f;
        }
        float t =
            Mathf.InverseLerp(
                sunriseColourStartHour,
                sunriseHour,
                hour
            );
        return Mathf.SmoothStep(
            1f,
            0f,
            t
        );
    }

    private Color GetSkyColour()
    {
        float hour = CurrentHour;
        if (hour >= sunriseHour &&
            hour < sunsetStartHour)
        {
            return daySkyColour;
        }
        if (hour >= sunsetStartHour &&
            hour < sunsetColourPeakHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunsetStartHour,
                    sunsetColourPeakHour,
                    hour
                );
            return Color.Lerp(
                daySkyColour,
                sunsetSkyColour,
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                )
            );
        }



        if (hour >= sunsetColourPeakHour && hour < sunsetEndHour)
        {
            float t =
                Mathf.InverseLerp(sunsetColourPeakHour,sunsetEndHour,hour);


            return Color.Lerp( sunsetSkyColour,nightSkyColour, Mathf.SmoothStep(0f,1f,t));
        }




        if (hour >= sunsetEndHour || hour < sunriseColourStartHour)
        {
            return nightSkyColour;
        }


        float sunriseT = Mathf.InverseLerp(sunriseColourStartHour,sunriseHour,hour);
        return Color.Lerp(nightSkyColour, daySkyColour, Mathf.SmoothStep(0f, 1f, sunriseT));
    }



    private Color GetHorizonColour()
    {
        float hour = CurrentHour;




        if (hour >= sunriseHour && hour < sunsetStartHour)
        {
            return dayHorizonColour;
        }




        if (hour >= sunsetStartHour &&
            hour < sunsetColourPeakHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunsetStartHour,
                    sunsetColourPeakHour,
                    hour
                );


            return Color.Lerp(
                dayHorizonColour,
                sunsetHorizonColour,
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                )
            );
        }



        if (hour >= sunsetColourPeakHour &&
            hour < sunsetEndHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunsetColourPeakHour,
                    sunsetEndHour,
                    hour
                );


            return Color.Lerp(
                sunsetHorizonColour,
                nightHorizonColour,
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                )
            );
        }




        if (hour >= sunsetEndHour ||
            hour < sunriseColourStartHour)
        {
            return nightHorizonColour;
        }



        float sunriseT =
            Mathf.InverseLerp(
                sunriseColourStartHour,
                sunriseHour,
                hour
            );


        return Color.Lerp(
            nightHorizonColour,
            dayHorizonColour,
            Mathf.SmoothStep(
                0f,
                1f,
                sunriseT
            )
        );
    }




    private Color GetGroundColour()
    {
        float hour = CurrentHour;




        if (hour >= sunriseHour &&
            hour < sunsetStartHour)
        {
            return dayGroundColour;
        }




        if (hour >= sunsetStartHour &&
            hour < sunsetColourPeakHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunsetStartHour,
                    sunsetColourPeakHour,
                    hour
                );


            return Color.Lerp(
                dayGroundColour,
                sunsetGroundColour,
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                )
            );
        }




        if (hour >= sunsetColourPeakHour &&
            hour < sunsetEndHour)
        {
            float t =
                Mathf.InverseLerp(
                    sunsetColourPeakHour,
                    sunsetEndHour,
                    hour
                );


            return Color.Lerp(
                sunsetGroundColour,
                nightGroundColour,
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                )
            );
        }




        if (hour >= sunsetEndHour ||
            hour < sunriseColourStartHour)
        {
            return nightGroundColour;
        }




        float sunriseT =
            Mathf.InverseLerp(
                sunriseColourStartHour,
                sunriseHour,
                hour
            );


        return Color.Lerp(
            nightGroundColour,
            dayGroundColour,
            Mathf.SmoothStep(
                0f,
                1f,
                sunriseT
            )
        );
    }




    private void UpdateSkybox()
    {
        if (skyboxMaterialInstance == null)
            return;



        if (skyboxMaterialInstance.HasProperty(
            dayNightSliderProperty))
        {
            skyboxMaterialInstance.SetFloat(
                dayNightSliderProperty,
                GetDayNightSlider()
            );
        }




        if (skyboxMaterialInstance.HasProperty(
            nightBlendProperty))
        {
            skyboxMaterialInstance.SetFloat(
                nightBlendProperty,
                GetNightBlend()
            );
        }




        if (skyboxMaterialInstance.HasProperty(
            skyColourProperty))
        {
            skyboxMaterialInstance.SetColor(
                skyColourProperty,
                GetSkyColour()
            );
        }




        if (skyboxMaterialInstance.HasProperty(
            nightSkyColourProperty))
        {
            skyboxMaterialInstance.SetColor(
                nightSkyColourProperty,
                nightSkyColour
            );
        }




        if (skyboxMaterialInstance.HasProperty(
            horizonColourProperty))
        {
            skyboxMaterialInstance.SetColor(
                horizonColourProperty,
                GetHorizonColour()
            );
        }




        if (skyboxMaterialInstance.HasProperty(
            groundColourProperty))
        {
            skyboxMaterialInstance.SetColor(
                groundColourProperty,
                GetGroundColour()
            );
        }






        if (skyboxMaterialInstance.HasProperty(
            starPowerProperty))
        {
            skyboxMaterialInstance.SetFloat(
                starPowerProperty,
                GetStarPower()
            );
        }
    }






    private float GetStarPower()
    {
        float hour = CurrentHour;
        if (hour >= starsFadeOutEndHour && hour < starsFadeInStartHour)
        {
            return dayStarPower;
        }
        if (hour >= starsFadeInStartHour && hour < starsFadeInEndHour)
        {
            float t = Mathf.InverseLerp( starsFadeInStartHour,starsFadeInEndHour,hour);


            return Mathf.Lerp( dayStarPower,nightStarPower,Mathf.SmoothStep(0f,1f,t));
        }

        if (hour >= starsFadeInEndHour || hour < starsFadeOutStartHour)
        {
            return nightStarPower;
        }

        float fadeOut = Mathf.InverseLerp(starsFadeOutStartHour, starsFadeOutEndHour,hour);
        return Mathf.Lerp(nightStarPower,dayStarPower,Mathf.SmoothStep(0f,1f,fadeOut));
    }

  private void OnDestroy()
    {
        if (skyboxMaterialInstance != null)
        {
            Destroy(skyboxMaterialInstance);
        }


        if (cloudMaterialInstance != null)
        {
            Destroy(cloudMaterialInstance);
        }
    }
}
using UnityEngine;

public class NightObjectController : MonoBehaviour
{

    [SerializeField] private Renderer objectRenderer;

    [SerializeField] private Material objectMaterial;

    [SerializeField] private Color dayColour = Color.white;

    [SerializeField] private Color nightColour = Color.blue;

    [SerializeField] private float colourChangeStartHour = 20f;
    [SerializeField] private float colourChangeEndHour = 21f;

 
    [SerializeField] private ParticleSystem nightParticleEffect;

    [SerializeField] private int nightStartHour = 21;
    [SerializeField] private int nightEndHour = 5;


    [SerializeField] private string colourProperty = "_BaseColor";

    private Material materialInstance;
    private bool isNight = false;

    private void Start()
    {
        SetupMaterial();

        UpdateNightState();
    }

    private void Update()
    {
        if (TimeController.instance == null)
            return;

        float currentHour =
            (float)TimeController.instance.CurrentTime.TimeOfDay.TotalHours;


        UpdateColour(currentHour);

        bool shouldBeNight =
            currentHour >= nightStartHour ||
            currentHour < nightEndHour;

        if (shouldBeNight != isNight)
        {
            isNight = shouldBeNight;

            if (isNight)
            {
                StartNight();
            }
            else
            {
                EndNight();
            }
        }
    }

    private void SetupMaterial()
    {
        if (objectRenderer == null)
            return;

        if (objectMaterial != null)
        {
            materialInstance = new Material(objectMaterial);
            objectRenderer.material = materialInstance;
        }
        else
        {

            materialInstance = new Material(objectRenderer.material);
            objectRenderer.material = materialInstance;
        }
    }

    private void UpdateNightState()
    {
        if (TimeController.instance == null)
            return;

        float currentHour =
            (float)TimeController.instance.CurrentTime.TimeOfDay.TotalHours;


        UpdateColour(currentHour);

     
        bool shouldBeNight =
            currentHour >= nightStartHour ||
            currentHour < nightEndHour;

        isNight = shouldBeNight;

        if (isNight)
        {
            StartNight();
        }
        else
        {
            EndNight();
        }
    }

    private void UpdateColour(float currentHour)
    {
        if (materialInstance == null)
            return;

        Color targetColour;

 
        if (currentHour < colourChangeStartHour)
        {
            targetColour = dayColour;
        }

    
        else if (currentHour >= colourChangeEndHour)
        {
            targetColour = nightColour;
        }


        else
        {
            float transition =
                Mathf.InverseLerp(
                    colourChangeStartHour,
                    colourChangeEndHour,
                    currentHour
                );

            targetColour =
                Color.Lerp(
                    dayColour,
                    nightColour,
                    transition
                );
        }

        ChangeMaterialColour(targetColour);
    }

    private void StartNight()
    {
        if (nightParticleEffect == null)
            return;

        if (!nightParticleEffect.isPlaying)
        {
            nightParticleEffect.Play();
        }
    }

    private void EndNight()
    {
        if (nightParticleEffect == null)
            return;

        if (nightParticleEffect.isPlaying)
        {
            nightParticleEffect.Stop();
        }
    }

    private void ChangeMaterialColour(Color newColour)
    {
        if (materialInstance == null)
            return;

        if (materialInstance.HasProperty(colourProperty))
        {
            materialInstance.SetColor(
                colourProperty,
                newColour
            );
        }
        else
        {
           
        }
    }
}


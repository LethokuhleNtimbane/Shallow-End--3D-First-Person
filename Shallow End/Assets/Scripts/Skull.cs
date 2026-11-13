using UnityEngine;

public class NightObjectController : MonoBehaviour
{
    [Header("Objects To Change")]
    [SerializeField] private Renderer[] objectsToChange;

    [Header("Monster Spawner")]
    [SerializeField] private MonsterSpawner monsterSpawner;

    [Header("Colours")]
    [SerializeField] private Color dayColour = Color.white;
    [SerializeField] private Color nightColour = Color.blue;
    [SerializeField] private string colourProperty = "_BaseColor";
    [SerializeField] private float colourTransitionDuration = 1f;

    [Header("Particle Effect")]
    [SerializeField] private ParticleSystem nightParticleEffect;

    private MaterialPropertyBlock[] propertyBlocks;
    private Color[] originalColours;

    private bool wasNight;
    private bool hasMonsters;

    private float transitionProgress;
    private Color transitionStartColour;
    private Color transitionTargetColour;
    private bool transitioning;

    private int colourPropertyID;

    private void Start()
    {
        colourPropertyID = Shader.PropertyToID(colourProperty);

        if (objectsToChange == null || objectsToChange.Length == 0)
            return;

        propertyBlocks = new MaterialPropertyBlock[objectsToChange.Length];
        originalColours = new Color[objectsToChange.Length];

        for (int i = 0; i < objectsToChange.Length; i++)
        {
            if (objectsToChange[i] == null)
                continue;

            propertyBlocks[i] = new MaterialPropertyBlock();

            objectsToChange[i].GetPropertyBlock(propertyBlocks[i]);

            Material material = objectsToChange[i].sharedMaterial;

            if (material != null && material.HasProperty(colourPropertyID))
                originalColours[i] = material.GetColor(colourPropertyID);
            else
                originalColours[i] = dayColour;

            SetObjectColour(i, dayColour);
        }

        if (nightParticleEffect != null)
            nightParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        TimeController timeController = TimeController.instance;

        if (timeController != null)
            wasNight = timeController.IsNight;
        else
            wasNight = false;

        ApplyColour(wasNight ? nightColour : dayColour);

        hasMonsters = false;
    }

    private void Update()
    {
        TimeController timeController = TimeController.instance;

        if (timeController != null)
        {
            bool isNight = timeController.IsNight;

            if (isNight != wasNight)
            {
                wasNight = isNight;

                StartColourTransition(
                    isNight ? nightColour : dayColour
                );
            }
        }

        UpdateMonsterParticle();

        if (transitioning)
            UpdateColourTransition();
    }

    private void UpdateMonsterParticle()
    {
        if (monsterSpawner == null)
            return;

        bool monstersPresent = monsterSpawner.HasActiveMonsters;

        if (monstersPresent != hasMonsters)
        {
            hasMonsters = monstersPresent;

            if (nightParticleEffect != null)
            {
                if (hasMonsters)
                {
                    if (!nightParticleEffect.isPlaying)
                        nightParticleEffect.Play();
                }
                else
                {
                    nightParticleEffect.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear
                    );
                }
            }
        }
    }

    private void StartColourTransition(Color targetColour)
    {
        transitionStartColour = GetCurrentColour();

        transitionTargetColour = targetColour;
        transitionProgress = 0f;
        transitioning = true;

        if (colourTransitionDuration <= 0f)
        {
            ApplyColour(targetColour);
            transitioning = false;
        }
    }

    private Color GetCurrentColour()
    {
        if (objectsToChange != null)
        {
            for (int i = 0; i < objectsToChange.Length; i++)
            {
                if (objectsToChange[i] == null)
                    continue;

                Material material = objectsToChange[i].sharedMaterial;

                if (material != null && material.HasProperty(colourPropertyID))
                    return material.GetColor(colourPropertyID);
            }
        }

        return wasNight ? nightColour : dayColour;
    }

    private void UpdateColourTransition()
    {
        if (colourTransitionDuration <= 0f)
            return;

        transitionProgress += Time.deltaTime / colourTransitionDuration;

        float progress = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.Clamp01(transitionProgress)
        );

        Color currentColour = Color.Lerp(
            transitionStartColour,
            transitionTargetColour,
            progress
        );

        ApplyColour(currentColour);

        if (transitionProgress >= 1f)
        {
            ApplyColour(transitionTargetColour);
            transitioning = false;
        }
    }

    private void ApplyColour(Color colour)
    {
        if (objectsToChange == null)
            return;

        for (int i = 0; i < objectsToChange.Length; i++)
        {
            SetObjectColour(i, colour);
        }
    }

    private void SetObjectColour(int index, Color colour)
    {
        if (objectsToChange == null ||
            index >= objectsToChange.Length ||
            objectsToChange[index] == null ||
            propertyBlocks == null ||
            propertyBlocks[index] == null)
            return;

        objectsToChange[index].GetPropertyBlock(propertyBlocks[index]);

        propertyBlocks[index].SetColor(
            colourPropertyID,
            colour
        );

        objectsToChange[index].SetPropertyBlock(
            propertyBlocks[index]
        );
    }

    private void OnDestroy()
    {
        if (objectsToChange == null || propertyBlocks == null)
            return;

        for (int i = 0; i < objectsToChange.Length; i++)
        {
            if (objectsToChange[i] != null)
                objectsToChange[i].SetPropertyBlock(null);
        }
    }
}
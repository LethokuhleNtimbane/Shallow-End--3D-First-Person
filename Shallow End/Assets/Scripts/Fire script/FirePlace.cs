using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Fireplace : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference lightFireAction;
    [SerializeField] private GameObject fireplaceInterface;
    [SerializeField] private GameObject fireplaceBackground;
    [SerializeField] private GameObject gameUIToHide;

    [Header("Player Scripts")]
    [SerializeField] private FoodScript foodScript;
    [SerializeField] private DashScript dash;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TimeController timeController;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private PauseScript pauseScript;

    [Header("Inventory")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Slot fuelSlot;
    [SerializeField] private Slot cookingSlot;

    [Header("Items")]
    [SerializeField] private Items woodItem;
    [SerializeField] private Items flintItem;
    [SerializeField] private Items waterItem;
    [SerializeField] private Items boiledWaterItem;
    [SerializeField] private Items rawFishItem;
    [SerializeField] private Items cookedFishItem;
    [SerializeField] private Items rawCrabItem;
    [SerializeField] private Items cookedCrabItem;

    [Header("Fire")]
    [SerializeField] private float fireDurationHours = 3f;
    [SerializeField] private GameObject fireVisual;
    [SerializeField] private Light fireLight;
    [SerializeField] private GameObject damageZone;
    [SerializeField] private GameObject protectionZone;
    [SerializeField] private FireProtect fireProtect;

    [Header("Fire Particles")]
    [SerializeField] private ParticleSystem fireParticle1;
    [SerializeField] private ParticleSystem fireParticle2;
    [SerializeField] private ParticleSystem fireParticle3;

    [Header("Cooking Visuals")]
    [SerializeField] private GameObject cookingSpinner;
    [SerializeField] private float cookingSpinnerSpeed = 100f;
    [SerializeField] private GameObject rawFishVisual;
    [SerializeField] private GameObject cookedFishVisual;
    [SerializeField] private GameObject rawCrabVisual;
    [SerializeField] private GameObject cookedCrabVisual;
    [SerializeField] private GameObject rawWaterVisual;
    [SerializeField] private GameObject boiledWaterVisual;

    [Header("Cooking Progress")]
    [SerializeField] private float fishCookingTimeHours = 1f;
    [SerializeField] private float crabCookingTimeHours = 1f;
    [SerializeField] private float waterBoilingTimeHours = 1f;
    [SerializeField] private Image cookingProgress;

    private bool interfaceOpen;
    private bool fireIsLit;

    private DateTime fireEndTime;
    private DateTime lastClockTime;

    private Items lastCookingItem;
    private float cookingElapsedHours;

    private Vector3 particle1OriginalScale;
    private Vector3 particle2OriginalScale;
    private Vector3 particle3OriginalScale;

    private float spinnerXRotation;

    private void Awake()
    {
        if (fireParticle1 != null)
            particle1OriginalScale = fireParticle1.transform.localScale;

        if (fireParticle2 != null)
            particle2OriginalScale = fireParticle2.transform.localScale;

        if (fireParticle3 != null)
            particle3OriginalScale = fireParticle3.transform.localScale;

        if (cookingSpinner != null)
            spinnerXRotation = cookingSpinner.transform.localEulerAngles.x;

        SetFireObjects(false);
        SetParticlesActive(false);
        SetCookingItemVisuals(null);

        if (fireplaceInterface != null)
            fireplaceInterface.SetActive(false);

        if (fireplaceBackground != null)
            fireplaceBackground.SetActive(false);

        if (gameUIToHide != null)
            gameUIToHide.SetActive(true);

        if (cookingProgress != null)
            cookingProgress.fillAmount = 0f;
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();

        if (lightFireAction != null)
            lightFireAction.action.Enable();

        if (inventory != null && fuelSlot != null && cookingSlot != null)
        {
            inventory.RegisterFireplaceSlots(
                fuelSlot,
                cookingSlot,
                woodItem,
                rawFishItem,
                rawCrabItem,
                waterItem
            );
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();

        if (lightFireAction != null)
            lightFireAction.action.Disable();

        if (inventory != null)
            inventory.UnregisterFireplaceSlots();

        if (interfaceOpen)
            CloseInterface();
    }

    private void Update()
    {
        if (TimeController.instance == null)
            return;

        DateTime currentTime = TimeController.instance.CurrentTime;

        CheckInteraction();
        CheckFireLighting();

        UpdateFire(currentTime);
        UpdateCookingClock(currentTime);
        UpdateCooking();
    }

    private void CheckInteraction()
    {
        if (interactAction == null ||
            !interactAction.action.WasPressedThisFrame())
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = new Ray(
            cam.transform.position,
            cam.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance))
        {
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                if (interfaceOpen)
                    CloseInterface();
                else
                    OpenInterface();
            }
        }
    }

    private void CheckFireLighting()
    {
        if (interfaceOpen)
            return;

        if (lightFireAction == null ||
            !lightFireAction.action.WasPressedThisFrame())
            return;

        if (fireIsLit)
            return;

        if (inventory == null ||
            !inventory.IsFlintEquipped(flintItem))
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = new Ray(
            cam.transform.position,
            cam.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance))
            return;

        if (hit.collider.transform != transform &&
            !hit.collider.transform.IsChildOf(transform))
            return;

        if (fuelSlot == null || !fuelSlot.Hasitem())
        {
            inventory.ShowInteractionMessage(
                "I need to get wood for fuel"
            );

            return;
        }

        if (fuelSlot.GetItem() != woodItem)
            return;

        LightFire();
    }

    private void SetPlayerScriptsEnabled(bool enabled)
    {
        if (foodScript != null)
            foodScript.enabled = enabled;

        if (dash != null)
            dash.enabled = enabled;

        if (playerController != null)
            playerController.enabled = enabled;

        if (timeController != null)
            timeController.enabled = enabled;

        if (healthScript != null)
            healthScript.enabled = enabled;

        if (pauseScript != null)
            pauseScript.enabled = enabled;
    }

    private void OpenInterface()
    {
        interfaceOpen = true;

        if (fireplaceInterface != null)
            fireplaceInterface.SetActive(true);

        if (fireplaceBackground != null)
            fireplaceBackground.SetActive(true);

        if (gameUIToHide != null)
            gameUIToHide.SetActive(false);

        SetPlayerScriptsEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseInterface()
    {
        interfaceOpen = false;

        if (fireplaceInterface != null)
            fireplaceInterface.SetActive(false);

        if (fireplaceBackground != null)
            fireplaceBackground.SetActive(false);

        if (gameUIToHide != null)
            gameUIToHide.SetActive(true);

        SetPlayerScriptsEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LightFire()
    {
        fireIsLit = true;

        DateTime currentTime =
            TimeController.instance.CurrentTime;

        fireEndTime =
            currentTime.AddHours(fireDurationHours);

        lastClockTime = currentTime;

        fuelSlot.ClearSlot();

        inventory.UseEquippedDurability();

        SetFireObjects(true);
        SetParticlesActive(true);
        ResetParticleScales();

        if (fireProtect != null)
            fireProtect.SetFire(true);
    }

    private void UpdateFire(DateTime currentTime)
    {
        if (!fireIsLit)
            return;

        if (currentTime >= fireEndTime)
        {
            ExtinguishFire();
            return;
        }

        UpdateFireParticles(currentTime);
    }

    private void ExtinguishFire()
    {
        fireIsLit = false;

        SetFireObjects(false);
        SetParticlesActive(false);
        ResetParticleScales();

        if (fireProtect != null)
            fireProtect.SetFire(false);
    }

    private void SetFireObjects(bool active)
    {
        if (fireVisual != null)
            fireVisual.SetActive(active);

        if (fireLight != null)
            fireLight.enabled = active;

        if (damageZone != null)
            damageZone.SetActive(active);

        if (protectionZone != null)
            protectionZone.SetActive(active);
    }

    private void SetParticlesActive(bool active)
    {
        ParticleSystem[] particles =
        {
            fireParticle1,
            fireParticle2,
            fireParticle3
        };

        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
                continue;

            if (active)
            {
                particle.gameObject.SetActive(true);
                particle.Play();
            }
            else
            {
                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                particle.gameObject.SetActive(false);
            }
        }
    }

    private void ResetParticleScales()
    {
        if (fireParticle1 != null)
            fireParticle1.transform.localScale =
                particle1OriginalScale;

        if (fireParticle2 != null)
            fireParticle2.transform.localScale =
                particle2OriginalScale;

        if (fireParticle3 != null)
            fireParticle3.transform.localScale =
                particle3OriginalScale;
    }

    private void UpdateFireParticles(DateTime currentTime)
    {
        double remainingSeconds =
            (fireEndTime - currentTime).TotalSeconds;

        float remainingPercentage =
            Mathf.Clamp01(
                (float)(
                    remainingSeconds /
                    (fireDurationHours * 3600f)
                )
            );

        if (fireParticle1 != null)
            fireParticle1.transform.localScale =
                particle1OriginalScale *
                remainingPercentage;

        if (fireParticle2 != null)
            fireParticle2.transform.localScale =
                particle2OriginalScale *
                remainingPercentage;

        if (fireParticle3 != null)
            fireParticle3.transform.localScale =
                particle3OriginalScale *
                remainingPercentage;
    }

    private void UpdateCookingClock(DateTime currentTime)
    {
        Items currentCookingItem =
            cookingSlot != null
                ? cookingSlot.GetItem()
                : null;

        if (currentCookingItem != lastCookingItem)
        {
            lastCookingItem = currentCookingItem;
            cookingElapsedHours = 0f;
            lastClockTime = currentTime;

            if (cookingProgress != null)
                cookingProgress.fillAmount = 0f;
        }

        if (currentCookingItem == null)
            return;

        if (!fireIsLit)
        {
            lastClockTime = currentTime;
            return;
        }

        DateTime effectiveCurrentTime =
            currentTime;

        if (effectiveCurrentTime > fireEndTime)
            effectiveCurrentTime = fireEndTime;

        double elapsedSeconds =
            (effectiveCurrentTime - lastClockTime).TotalSeconds;

        if (elapsedSeconds > 0)
        {
            cookingElapsedHours +=
                (float)elapsedSeconds /
                3600f;
        }

        lastClockTime = effectiveCurrentTime;
    }

    private void UpdateCooking()
    {
        if (cookingSlot == null ||
            !cookingSlot.Hasitem())
        {
            SetCookingItemVisuals(null);

            if (cookingProgress != null)
                cookingProgress.fillAmount = 0f;

            return;
        }

        Items currentItem =
            cookingSlot.GetItem();

        SetCookingItemVisuals(currentItem);

        if (!IsValidCookingItem(currentItem))
        {
            if (cookingProgress != null)
                cookingProgress.fillAmount = 0f;

            return;
        }

        bool isRawFishOrCrab =
            currentItem == rawFishItem ||
            currentItem == rawCrabItem;

        if (isRawFishOrCrab &&
            fireIsLit &&
            cookingSpinner != null)
        {
            spinnerXRotation +=
                cookingSpinnerSpeed *
                Time.deltaTime;

            Vector3 currentRotation =
                cookingSpinner.transform.localEulerAngles;

            currentRotation.x =
                spinnerXRotation;

            cookingSpinner.transform.localEulerAngles =
                currentRotation;
        }

        float cookingDuration =
            GetCookingDuration(currentItem);

        float progress =
            cookingDuration > 0f
                ? Mathf.Clamp01(
                    cookingElapsedHours /
                    cookingDuration
                )
                : 0f;

        if (cookingProgress != null)
            cookingProgress.fillAmount =
                progress;

        if (fireIsLit &&
            cookingDuration > 0f &&
            cookingElapsedHours >=
                cookingDuration)
        {
            FinishCooking();
        }
    }

    private void SetCookingItemVisuals(Items item)
    {
        if (rawFishVisual != null)
            rawFishVisual.SetActive(
                item == rawFishItem
            );

        if (cookedFishVisual != null)
            cookedFishVisual.SetActive(
                item == cookedFishItem
            );

        if (rawCrabVisual != null)
            rawCrabVisual.SetActive(
                item == rawCrabItem
            );

        if (cookedCrabVisual != null)
            cookedCrabVisual.SetActive(
                item == cookedCrabItem
            );

        if (rawWaterVisual != null)
            rawWaterVisual.SetActive(
                item == waterItem
            );

        if (boiledWaterVisual != null)
            boiledWaterVisual.SetActive(
                item == boiledWaterItem
            );
    }

    private bool IsValidCookingItem(Items item)
    {
        return item == waterItem ||
               item == rawFishItem ||
               item == rawCrabItem ||
               item == boiledWaterItem ||
               item == cookedFishItem ||
               item == cookedCrabItem;
    }

    private float GetCookingDuration(Items item)
    {
        if (item == rawFishItem)
            return fishCookingTimeHours;

        if (item == rawCrabItem)
            return crabCookingTimeHours;

        if (item == waterItem)
            return waterBoilingTimeHours;

        return 0f;
    }

    private void FinishCooking()
    {
        if (cookingSlot == null)
            return;

        Items currentItem =
            cookingSlot.GetItem();

        Items cookedItem = null;

        if (currentItem == rawFishItem)
            cookedItem = cookedFishItem;
        else if (currentItem == rawCrabItem)
            cookedItem = cookedCrabItem;
        else if (currentItem == waterItem)
            cookedItem = boiledWaterItem;

        if (cookedItem == null)
            return;

        int amount =
            cookingSlot.GetAmount();

        int durability =
            cookingSlot.GetDurability();

        cookingSlot.SetItem(
            cookedItem,
            amount,
            durability
        );

        cookingElapsedHours = 0f;
        lastCookingItem = cookedItem;

        if (cookingProgress != null)
            cookingProgress.fillAmount = 0f;

        SetCookingItemVisuals(cookedItem);
    }
}
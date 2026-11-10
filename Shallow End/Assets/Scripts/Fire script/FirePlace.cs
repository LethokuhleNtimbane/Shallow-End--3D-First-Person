
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Fireplace : MonoBehaviour
{
    [Header("Fire Objects")]
    [SerializeField] private GameObject fireMat;
    [SerializeField] private Light fireLight;
    [SerializeField] private GameObject fireDamageZone;
    [SerializeField] private GameObject protectionZone;
    [SerializeField] private FireProtect fireProtection;

    [Header("Fire Settings")]
    [SerializeField] private float fireDurationInGameHours = 3f;
    [SerializeField] private ParticleSystem[] fireParticles;
    [SerializeField] private float minimumParticleScale = 0.2f;

    [Header("Inventory")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Slot fuelSlot;
    [SerializeField] private Slot cookingSlot;
    [SerializeField] private Items flintItem;
    [SerializeField] private Items woodItem;
    [SerializeField] private Items fishItem;
    [SerializeField] private Items crabItem;
    [SerializeField] private Items waterItem;

    [Header("Cooked Items")]
    [SerializeField] private Items cookedFishItem;
    [SerializeField] private Items cookedCrabItem;
    [SerializeField] private Items boiledWaterItem;

    [Header("Fireplace Interface")]
    [SerializeField] private GameObject fireplaceInterface;
    [SerializeField] private TextMeshProUGUI fuelMessage;
    [SerializeField] private float interactionRange = 3f;

    [Header("Cooking")]
    [SerializeField] private GameObject cookingVisual;
    [SerializeField] private Image cookingProgressImage;
    [SerializeField] private float fishCookingHours = 1f;
    [SerializeField] private float crabCookingHours = 1f;
    [SerializeField] private float waterBoilingHours = 1f;

    [Header("Cooking Spinner")]
    [SerializeField] private GameObject cookingSpinner;
    [SerializeField] private float spinnerRotationSpeed = 180f;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference lightFireAction;

    private bool fireIsLit;
    private bool interfaceOpen;
    private DateTime fireEndTime;
    private DateTime lastClockTime;
    private float cookingElapsedHours;
    private Items trackedCookingItem;
    private Vector3[] originalParticleScales;

    private AudioManager audioManager;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisibility;

    private void Awake()
    {
        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }

        CacheParticleScales();
    }

    private void Start()
    {
        if (fireplaceInterface != null)
        {
            fireplaceInterface.SetActive(false);
        }

        TurnFireOff();
        UpdateCookingVisuals();
    }

    private void OnEnable()
    {
        EnableAction(interactAction);
        EnableAction(lightFireAction);

        if (inventory != null)
        {
            inventory.RegisterFireplaceSlots(
                fuelSlot,
                cookingSlot,
                woodItem,
                fishItem,
                crabItem,
                waterItem
            );
        }
    }

    private void OnDisable()
    {
        DisableAction(interactAction);
        DisableAction(lightFireAction);

        if (interfaceOpen)
        {
            CloseInterface();
        }

        if (inventory != null)
        {
            inventory.UnregisterFireplaceSlots();
        }
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null &&
            actionReference.action != null)
        {
            actionReference.action.Enable();
        }
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null &&
            actionReference.action != null)
        {
            actionReference.action.Disable();
        }
    }

    private void Update()
    {
        if (TimeController.instance == null)
            return;

        DateTime currentTime =
            TimeController.instance.CurrentTime;

        UpdateCookingClock(currentTime);
        UpdateCooking();

        if (fireIsLit)
        {
            UpdateParticleScales(currentTime);

            if (currentTime >= fireEndTime)
            {
                TurnFireOff();
            }
        }

        UpdateCookingVisuals();
        CheckInteraction();
        CheckFireLighting();
        UpdateFuelMessage();
    }

    private void CheckInteraction()
    {
        if (PlayerController.Instance == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            PlayerController.Instance.transform.position
        );

        if (distance > interactionRange && !interfaceOpen)
            return;

        if (interactAction == null ||
            !interactAction.action.WasPressedThisFrame())
            return;

        if (interfaceOpen)
        {
            CloseInterface();
        }
        else if (distance <= interactionRange)
        {
            OpenInterface();
        }
    }

    private void OpenInterface()
    {
        if (fireplaceInterface == null)
            return;

        interfaceOpen = true;
        fireplaceInterface.SetActive(true);

        previousCursorLockState = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseInterface()
    {
        interfaceOpen = false;

        if (fireplaceInterface != null)
        {
            fireplaceInterface.SetActive(false);
        }

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisibility;
    }

    private void CheckFireLighting()
    {
        if (fireIsLit || interfaceOpen)
            return;

        if (PlayerController.Instance == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            PlayerController.Instance.transform.position
        );

        if (distance > interactionRange)
            return;

        if (lightFireAction == null ||
            !lightFireAction.action.WasPressedThisFrame())
            return;

        if (inventory == null ||
            !inventory.IsFlintEquipped(flintItem))
            return;

        if (fuelSlot == null ||
            !fuelSlot.Hasitem() ||
            fuelSlot.GetItem() != woodItem)
        {
            if (fuelMessage != null)
            {
                fuelMessage.text = "I need to get wood for fuel";
                fuelMessage.gameObject.SetActive(true);
            }

            return;
        }

        LightFire();
    }

    private void LightFire()
    {
        if (TimeController.instance == null)
            return;

        fireIsLit = true;

        lastClockTime = TimeController.instance.CurrentTime;

        fireEndTime = lastClockTime.AddHours(
            Mathf.Max(0.01f, fireDurationInGameHours)
        );

        fuelSlot.RemoveAmount(1);

        if (fireMat != null)
        {
            fireMat.SetActive(true);
        }

        if (fireLight != null)
        {
            fireLight.enabled = true;
        }

        if (fireDamageZone != null)
        {
            fireDamageZone.SetActive(true);
        }

        if (protectionZone != null)
        {
            protectionZone.SetActive(true);
        }

        if (fireProtection != null)
        {
            fireProtection.SetFire(true);
        }

        inventory.UseEquippedDurability();

        if (audioManager != null)
        {
            audioManager.PlaySfx(audioManager.FlintLight);
        }

        ResetParticleScales();
        SetParticlesActive(true);
        UpdateFuelMessage();
    }

    private void TurnFireOff()
    {
        fireIsLit = false;

        if (fireMat != null)
        {
            fireMat.SetActive(false);
        }

        if (fireLight != null)
        {
            fireLight.enabled = false;
        }

        if (fireDamageZone != null)
        {
            fireDamageZone.SetActive(false);
        }

        if (protectionZone != null)
        {
            protectionZone.SetActive(false);
        }

        if (fireProtection != null)
        {
            fireProtection.SetFire(false);
        }

        SetParticlesActive(false);
        SetSpinnerActive(false);
    }

    private void CacheParticleScales()
    {
        if (fireParticles == null)
            return;

        originalParticleScales =
            new Vector3[fireParticles.Length];

        for (int i = 0; i < fireParticles.Length; i++)
        {
            if (fireParticles[i] != null)
            {
                originalParticleScales[i] =
                    fireParticles[i].transform.localScale;
            }
        }
    }

    private void ResetParticleScales()
    {
        if (fireParticles == null ||
            originalParticleScales == null)
            return;

        for (int i = 0; i < fireParticles.Length; i++)
        {
            if (fireParticles[i] != null)
            {
                fireParticles[i].transform.localScale =
                    originalParticleScales[i];
            }
        }
    }

    private void UpdateParticleScales(DateTime currentTime)
    {
        if (fireParticles == null ||
            originalParticleScales == null)
            return;

        double remainingHours =
            (fireEndTime - currentTime).TotalHours;

        float remainingPercentage = Mathf.Clamp01(
            (float)(remainingHours /
            Mathf.Max(0.01f, fireDurationInGameHours))
        );

        float scaleMultiplier = Mathf.Lerp(
            minimumParticleScale,
            1f,
            remainingPercentage
        );

        for (int i = 0; i < fireParticles.Length; i++)
        {
            if (fireParticles[i] != null)
            {
                fireParticles[i].transform.localScale =
                    originalParticleScales[i] * scaleMultiplier;
            }
        }
    }

    private void SetParticlesActive(bool active)
    {
        if (fireParticles == null)
            return;

        foreach (ParticleSystem particle in fireParticles)
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
                particle.Stop();
                particle.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateCookingClock(DateTime currentTime)
    {
        Items currentItem =
            cookingSlot != null && cookingSlot.Hasitem()
                ? cookingSlot.GetItem()
                : null;

        if (currentItem != trackedCookingItem)
        {
            trackedCookingItem = currentItem;
            cookingElapsedHours = 0f;
            lastClockTime = currentTime;
        }

        if (currentItem == null ||
            !IsRawCookingItem(currentItem))
        {
            lastClockTime = currentTime;
            return;
        }

        if (fireIsLit)
        {
            DateTime effectiveTime = currentTime;

            if (effectiveTime > fireEndTime)
            {
                effectiveTime = fireEndTime;
            }

            if (effectiveTime > lastClockTime)
            {
                cookingElapsedHours += (float)(
                    effectiveTime - lastClockTime
                ).TotalHours;
            }
        }

        lastClockTime = currentTime;
    }

    private void UpdateCooking()
    {
        if (cookingSlot == null ||
            !cookingSlot.Hasitem())
        {
            cookingElapsedHours = 0f;
            trackedCookingItem = null;

            if (cookingProgressImage != null)
            {
                cookingProgressImage.fillAmount = 0f;
            }

            SetSpinnerActive(false);
            return;
        }

        Items currentItem = cookingSlot.GetItem();

        if (!IsRawCookingItem(currentItem))
        {
            if (cookingProgressImage != null)
            {
                cookingProgressImage.fillAmount = 1f;
            }

            SetSpinnerActive(false);
            return;
        }

        float cookingDuration = Mathf.Max(
            0.01f,
            GetCookingDuration(currentItem)
        );

        float progress = Mathf.Clamp01(
            cookingElapsedHours / cookingDuration
        );

        if (cookingProgressImage != null)
        {
            cookingProgressImage.fillAmount = progress;
        }

        bool shouldRotate =
            fireIsLit &&
            (currentItem == fishItem ||
             currentItem == crabItem);

        SetSpinnerActive(shouldRotate);

        if (cookingElapsedHours >= cookingDuration)
        {
            FinishCooking(currentItem);
        }
    }

    private void FinishCooking(Items rawItem)
    {
        Items cookedItem = GetCookedResult(rawItem);

        if (cookedItem == null)
            return;

        cookingSlot.SetItem(cookedItem, 1);

        trackedCookingItem = cookedItem;
        cookingElapsedHours = 0f;

        if (cookingProgressImage != null)
        {
            cookingProgressImage.fillAmount = 1f;
        }

        SetSpinnerActive(false);
    }

    private bool IsRawCookingItem(Items item)
    {
        return item != null &&
               (item == fishItem ||
                item == crabItem ||
                item == waterItem);
    }

    private Items GetCookedResult(Items rawItem)
    {
        if (rawItem == fishItem)
            return cookedFishItem;

        if (rawItem == crabItem)
            return cookedCrabItem;

        if (rawItem == waterItem)
            return boiledWaterItem;

        return null;
    }

    private float GetCookingDuration(Items item)
    {
        if (item == fishItem)
            return fishCookingHours;

        if (item == crabItem)
            return crabCookingHours;

        if (item == waterItem)
            return waterBoilingHours;

        return 0f;
    }

    private void UpdateCookingVisuals()
    {
        bool hasCookingItem =
            cookingSlot != null &&
            cookingSlot.Hasitem();

        if (cookingVisual != null)
        {
            cookingVisual.SetActive(hasCookingItem);
        }
    }

    private void SetSpinnerActive(bool active)
    {
        if (cookingSpinner != null &&
            cookingSpinner.activeSelf != active)
        {
            cookingSpinner.SetActive(active);
        }
    }

    private void UpdateFuelMessage()
    {
        if (fuelMessage == null)
            return;

        bool hasWood =
            fuelSlot != null &&
            fuelSlot.Hasitem() &&
            fuelSlot.GetItem() == woodItem;

        if (hasWood)
        {
            fuelMessage.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!fireIsLit ||
            cookingSpinner == null ||
            !cookingSpinner.activeSelf ||
            cookingSlot == null ||
            !cookingSlot.Hasitem())
            return;

        Items item = cookingSlot.GetItem();

        if (item != fishItem && item != crabItem)
            return;

        cookingSpinner.transform.Rotate(
            Vector3.right,
            spinnerRotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}

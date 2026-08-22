using UnityEngine;
using UnityEngine.InputSystem;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera sleepCamera;
    [SerializeField] private Monster monster;
    [SerializeField] private float sleepTimeMultiplier = 120f;

    [SerializeField] private InputActionReference interactAction;

    private bool sleeping = false;

    public bool IsSleeping => sleeping;

    private float normalTimeMultiplier;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        sleepCamera.gameObject.SetActive(false);

        normalTimeMultiplier = GetTimeMultiplier();

        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
        {
            return;
        }
    }

    private void Update()
    {
        if (!sleeping)
            return;

        // Wake up by pressing Space
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            WakeUp();
            return;
        }

        // Automatically wake up at 6 AM
        CheckWakeUp();
    }

    public void TrySleep()
    {
        if (sleeping)
            return;

        TimeController time = TimeController.instance;

        if (time == null)
            return;

        int hour = time.CurrentTime.Hour;

        // Can sleep from 21:00 until 05:59
        if (hour < 21 && hour >= 6)
        {
            Debug.Log("You can only sleep between 21:00 and 06:00.");
            return;
        }

        StartSleeping();
    }

    private void StartSleeping()
    {
        sleeping = true;

        // Disable monster
        if (monster != null)
        {
            monster.SetPlayerSleeping(true);
        }

        // Disable player movement and camera look
        PlayerController.Instance.PlayerControl(false);

        // Switch cameras
        playerCamera.gameObject.SetActive(false);
        sleepCamera.gameObject.SetActive(true);

        // Speed up time
        TimeController.instance.SetTimeMultiplier(sleepTimeMultiplier);
    }

    private void CheckWakeUp()
    {
        TimeController time = TimeController.instance;

        if (time.CurrentTime.Hour >= 6 &&
            time.CurrentTime.Hour < 21)
        {
            WakeUp();
        }
    }

    private void WakeUp()
    {
        if (!sleeping)
            return;

        sleeping = false;

        // Restore normal time speed
        TimeController.instance.SetTimeMultiplier(normalTimeMultiplier);

        // Switch cameras back
        sleepCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        // Give player control back
        PlayerController.Instance.PlayerControl(true);

        // Tell monster player is awake
        if (monster != null)
        {
            monster.SetPlayerSleeping(false);
        }
    }
    private float GetTimeMultiplier()
    {
        return TimeController.instance != null
            ? GetPrivateTimeMultiplier()
            : 1f;
    }

    private float GetPrivateTimeMultiplier()
    {
        return 1f;
    }

    private void SetTimeMultiplier(float value)
    {
        TimeController.instance.SetTimeMultiplier(value);
    }
}
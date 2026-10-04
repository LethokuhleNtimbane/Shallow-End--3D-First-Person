using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera sleepCamera;
    [SerializeField] private GameObject sleepObject;

    [SerializeField] private TextMeshProUGUI sleepWarningText;
    [SerializeField] private float warningDuration = 2f;

    [SerializeField] private float sleepTimeMultiplier = 240f;
    [SerializeField] private float normalTimeMultiplier = 60f;

    private bool sleeping = false;
    private Coroutine warningCoroutine;

    public bool IsSleeping => sleeping;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (sleepCamera != null)
            sleepCamera.gameObject.SetActive(false);

        if (sleepObject != null)
            sleepObject.SetActive(false);

        if (sleepWarningText != null)
            sleepWarningText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!sleeping)
            return;

    
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            WakeUp();
            return;
        }

        TimeController time = TimeController.instance;

        if (time == null)
            return;

      
        if (time.CurrentTime.Hour >= 6 &&
            time.CurrentTime.Hour < 21)
        {
            WakeUp();
        }
    }

    public void TrySleep()
    {
        if (sleeping)
            return;

        TimeController time = TimeController.instance;

        if (time == null)
            return;

        int hour = time.CurrentTime.Hour;

        if (hour >= 6 && hour < 21)
        {
            ShowSleepWarning();
            return;
        }

        StartSleeping();
    }

    private void ShowSleepWarning()
    {
        if (sleepWarningText == null)
            return;

        sleepWarningText.text = "I can only sleep at 21:00";
        sleepWarningText.gameObject.SetActive(true);

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningCoroutine = StartCoroutine(HideSleepWarning());
    }

    private IEnumerator HideSleepWarning()
    {
        yield return new WaitForSeconds(warningDuration);

        if (sleepWarningText != null)
            sleepWarningText.gameObject.SetActive(false);

        warningCoroutine = null;
    }

    private void StartSleeping()
    {
        sleeping = true;

        if (sleepObject != null)
            sleepObject.SetActive(true);

        if (PlayerController.Instance != null)
            PlayerController.Instance.PlayerControl(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (sleepCamera != null)
            sleepCamera.gameObject.SetActive(true);

    
        DisableAllMonsters();

   
        if (TimeController.instance != null)
        {
            TimeController.instance.SetTimeMultiplier(
                sleepTimeMultiplier
            );
        }
    }

    private void DisableAllMonsters()
    {
        Monster[] monsters =
            FindObjectsByType<Monster>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Monster monster in monsters)
        {
            if (monster != null)
            {
                
                monster.gameObject.SetActive(false);
            }
        }
    }

    private void EnableAllMonsters()
    {
        Monster[] monsters =
            FindObjectsByType<Monster>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Monster monster in monsters)
        {
            if (monster != null)
            {
             
                monster.gameObject.SetActive(true);
            }
        }
    }

    public void MonsterWakePlayer(float damage)
    {
        if (!sleeping)
            return;

        HealthScript health = FindPlayerHealth();

        if (health != null)
            health.TakeDamage(damage);

        WakeUp();
    }

    private HealthScript FindPlayerHealth()
    {
        GameObject player = GameObject.Find("Player");

        if (player == null)
            return null;

        return player.GetComponent<HealthScript>();
    }

    private void WakeUp()
    {
        if (!sleeping)
            return;

        sleeping = false;

        if (sleepObject != null)
            sleepObject.SetActive(false);

        if (TimeController.instance != null)
        {
            TimeController.instance.SetTimeMultiplier(
                normalTimeMultiplier
            );
        }

        if (sleepCamera != null)
            sleepCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (PlayerController.Instance != null)
            PlayerController.Instance.PlayerControl(true);

        
        EnableAllMonsters();
    }
}
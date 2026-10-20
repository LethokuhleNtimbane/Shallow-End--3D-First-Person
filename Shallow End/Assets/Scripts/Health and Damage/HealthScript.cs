
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthScript : MonoBehaviour
{
    [Header("Health")]
    public Image image;
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("Blood Splatter")]
    [SerializeField] private CanvasGroup bloodSplatter;
    [SerializeField] private float bloodIntensity = 1.5f;
    [SerializeField] private float bloodFadeSpeed = 2f;

    [Header("Hunger")]
    public Image hungerImage;
    public float hunger = 100f;
    public float maxHunger = 100f;

    [SerializeField] private float hungerLossAmount = 1f;
    [SerializeField] private float hungerLossTimer = 10f;

    private float hungerCooldown;

    [Header("Thirst")]
    public Image thirstImage;
    public float thirst = 100f;
    public float maxThirst = 100f;

    [SerializeField] private float thirstLossAmount = 1f;
    [SerializeField] private float thirstLossTimer = 7f;

    private float thirstCooldown;

    [Header("Health Regeneration")]
    [SerializeField] private float maxHealthRegen = 5f;
    [SerializeField] private float regenTimer = 2f;

    private float regenCooldown;

    [Header("Starvation Damage")]
    [SerializeField] private float starvationDamage = 2f;
    [SerializeField] private float starvationDamageTimer = 3f;

    private float starvationDamageCooldown;

    [Header("Status Effects")]
    [SerializeField] private bool healthRegenerationEnabled = true;

    public bool CanRegenerateHealth => healthRegenerationEnabled;

    private void Start()
    {
        hungerCooldown = hungerLossTimer;
        thirstCooldown = thirstLossTimer;
        regenCooldown = regenTimer;
        starvationDamageCooldown = starvationDamageTimer;

        UpdateHealthBar();
        UpdateHungerBar();
        UpdateThirstBar();
        UpdateBloodSplatter();
    }

    private void Update()
    {
        hungerCooldown -= Time.deltaTime;

        if (hungerCooldown <= 0f)
        {
            RemoveHunger(hungerLossAmount);
            hungerCooldown = hungerLossTimer;
        }

        thirstCooldown -= Time.deltaTime;

        if (thirstCooldown <= 0f)
        {
            RemoveThirst(thirstLossAmount);
            thirstCooldown = thirstLossTimer;
        }

        if (hunger <= 0f || thirst <= 0f)
        {
            starvationDamageCooldown -= Time.deltaTime;

            if (starvationDamageCooldown <= 0f)
            {
                TakeDamage(starvationDamage);
                starvationDamageCooldown = starvationDamageTimer;
            }
        }
        else
        {
            starvationDamageCooldown = starvationDamageTimer;
        }

        // Poison prevents regeneration regardless of hunger or thirst.
        if (healthRegenerationEnabled && health < maxHealth)
        {
            regenCooldown -= Time.deltaTime;

            if (regenCooldown <= 0f)
            {
                RegenerateHealth();
                regenCooldown = regenTimer;
            }
        }

        UpdateBloodSplatter();
    }

    public void SetHealthRegenerationEnabled(bool enabled)
    {
        healthRegenerationEnabled = enabled;

        // Avoid an immediate regeneration tick when re-enabled.
        if (enabled)
        {
            regenCooldown = regenTimer;
        }
    }

    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0f, maxHealth);
        UpdateHealthBar();
    }

    public void AddHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        UpdateHealthBar();
    }

    public bool PlayerIsFullHealth()
    {
        return health >= maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health = Mathf.Clamp(health - damage, 0f, maxHealth);
        UpdateHealthBar();
    }

    private void RegenerateHealth()
    {
        float hungerPercentage = maxHunger > 0f
            ? hunger / maxHunger : 0f;

        float thirstPercentage = maxThirst > 0f
            ? thirst / maxThirst : 0f;

        float averagePercentage =
            (hungerPercentage + thirstPercentage) / 2f;

        float regenerationAmount =
            maxHealthRegen * averagePercentage;

        AddHealth(regenerationAmount);
    }

    public void RemoveHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger - amount, 0f, maxHunger);
        UpdateHungerBar();
    }

    public void AddHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
        UpdateHungerBar();
    }

    public bool PlayerIsFullHunger()
    {
        return hunger >= maxHunger;
    }

    public void RemoveThirst(float amount)
    {
        thirst = Mathf.Clamp(thirst - amount, 0f, maxThirst);
        UpdateThirstBar();
    }

    public void AddThirst(float amount)
    {
        thirst = Mathf.Clamp(thirst + amount, 0f, maxThirst);
        UpdateThirstBar();
    }

    public bool PlayerIsFullThirst()
    {
        return thirst >= maxThirst;
    }

    private void UpdateHealthBar()
    {
        if (image != null)
        {
            image.fillAmount = maxHealth > 0f
                ? health / maxHealth : 0f;
        }
    }

    private void UpdateBloodSplatter()
    {
        if (bloodSplatter == null)
            return;

        float healthPercentage = maxHealth > 0f
            ? Mathf.Clamp01(health / maxHealth)
            : 0f;

        float targetAlpha = Mathf.Pow(
            1f - healthPercentage,
            bloodIntensity
        );

        bloodSplatter.alpha = Mathf.MoveTowards(
            bloodSplatter.alpha,
            targetAlpha,
            bloodFadeSpeed * Time.deltaTime
        );
    }

    private void UpdateHungerBar()
    {
        if (hungerImage != null)
        {
            hungerImage.fillAmount = maxHunger > 0f
                ? hunger / maxHunger : 0f;
        }
    }

    private void UpdateThirstBar()
    {
        if (thirstImage != null)
        {
            thirstImage.fillAmount = maxThirst > 0f
                ? thirst / maxThirst : 0f;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthScript : MonoBehaviour
{


    public Image image;

    public float health = 100f;
    public float maxHealth = 100f;






    public Image hungerImage;

    public float hunger = 100f;
    public float maxHunger = 100f;

  

    [SerializeField] private float hungerLossAmount = 1f;
    [SerializeField] private float hungerLossTimer = 10f;

    private float hungerCooldown;



    public Image thirstImage;

    public float thirst = 100f;
    public float maxThirst = 100f;



    [SerializeField] private float thirstLossAmount = 1f;
    [SerializeField] private float thirstLossTimer = 7f;

    private float thirstCooldown;


 

    [SerializeField] private float maxHealthRegen = 5f;
    [SerializeField] private float regenTimer = 2f;

    private float regenCooldown;




    [SerializeField] private float starvationDamage = 2f;
    [SerializeField] private float starvationDamageTimer = 3f;

    private float starvationDamageCooldown;


 
    public void Start()
    {
        hungerCooldown = hungerLossTimer;
        thirstCooldown = thirstLossTimer;
        regenCooldown = regenTimer;
        starvationDamageCooldown = starvationDamageTimer;

        UpdateHealthBar();
        UpdateHungerBar();
        UpdateThirstBar();
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


       
        if (health < maxHealth)
        {
            regenCooldown -= Time.deltaTime;

            if (regenCooldown <= 0f)
            {
                RegenerateHealth();
                regenCooldown = regenTimer;
            }
        }
    }



    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0f, maxHealth);

        UpdateHealthBar();
    }


    public void AddHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        UpdateHealthBar();
    }


    public bool PlayerIsFullHealth()
    {
        return health >= maxHealth;
    }


    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        UpdateHealthBar();
    }




    private void RegenerateHealth()
    {
  
        float hungerPercentage = hunger / maxHunger;
        float thirstPercentage = thirst / maxThirst;

      
        float averagePercentage = (hungerPercentage + thirstPercentage) / 2f;

   
        float regenerationAmount = maxHealthRegen * averagePercentage;

        AddHealth(regenerationAmount);
    }


    public void RemoveHunger(float amount)
    {
        hunger -= amount;
        hunger = Mathf.Clamp(hunger, 0f, maxHunger);

        UpdateHungerBar();
    }


    public void AddHunger(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, maxHunger);

        UpdateHungerBar();
    }


    public bool PlayerIsFullHunger()
    {
        return hunger >= maxHunger;
    }


   

    public void RemoveThirst(float amount)
    {
        thirst -= amount;
        thirst = Mathf.Clamp(thirst, 0f, maxThirst);

        UpdateThirstBar();
    }


    public void AddThirst(float amount)
    {
        thirst += amount;
        thirst = Mathf.Clamp(thirst, 0f, maxThirst);

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
            image.fillAmount = health / maxHealth;
        }

    
    }


    private void UpdateHungerBar()
    {
        if (hungerImage != null)
        {
            hungerImage.fillAmount = hunger / maxHunger;
        }

    }


    private void UpdateThirstBar()
    {
        if (thirstImage != null)
        {
            thirstImage.fillAmount = thirst / maxThirst;
        }

    }
}
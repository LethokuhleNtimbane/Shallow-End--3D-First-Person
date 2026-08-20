using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthScript : MonoBehaviour
{
    public Image image;
    public float health = 100f;
    public float maxHealth = 100f;

    public TextMeshProUGUI healthText;

    public void Start()
    {
        UpdateHealthBar();
    }
    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0f, maxHealth);

        UpdateHealthBar();
      
    }
    public void Addhealth(float amount)
    {

        health += amount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        UpdateHealthBar();

    }
    public bool playerIsFullHealth()
    {
        return health >= maxHealth;
    }

  
    private void UpdateHealthBar()
    {
        if (image != null)
        {
           image.fillAmount = health / maxHealth;
          
        }

        if (healthText != null)
        {
            healthText.text =  Mathf.RoundToInt(health).ToString();        
        }

        
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);
        SetHealth(health);
    }
}

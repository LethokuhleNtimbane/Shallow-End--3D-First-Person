using UnityEngine;

public class FireHurts : MonoBehaviour
{
  
    [SerializeField] private float damage = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
           
            return;
        }
        HealthScript health = other.GetComponent<HealthScript>();

        if (health == null)
        {
       
            return;
        }

        health.TakeDamage(damage);

      
    }

}
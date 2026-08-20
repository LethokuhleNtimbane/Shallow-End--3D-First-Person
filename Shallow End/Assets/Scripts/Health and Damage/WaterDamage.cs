using System.Collections;
using UnityEngine;

public class WaterDamage : MonoBehaviour
{
    [SerializeField] private float damage = 30f;
    [SerializeField] private float SharkWaitTime = 3f;
  
    [SerializeField] private HealthScript health;

    private bool InWater;
    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InWater = true;
            Debug.Log("Player in Water");

            damageCoroutine = StartCoroutine(WaitAndDamage());
        }

     
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InWater = false;

            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    IEnumerator WaitAndDamage()
    {
        while (InWater)
        {
            yield return new WaitForSeconds(SharkWaitTime);

            if (InWater)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
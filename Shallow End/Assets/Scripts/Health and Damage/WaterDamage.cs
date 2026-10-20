
using System.Collections;
using UnityEngine;

public class WaterDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float SharkWaitTime = 3f;

    [Header("References")]
    [SerializeField] private HealthScript health;


    private bool InWater;
    private AudioManager audioManager;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
        {
            audioManager =
                audioObject.GetComponent<AudioManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || InWater)
        {
            return;
        }

        InWater = true;
        damageCoroutine = StartCoroutine(WaitAndDamage());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        InWater = false;

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator WaitAndDamage()
    {
        while (InWater)
        {
            yield return new WaitForSeconds(SharkWaitTime);

            if (!InWater)
            {
                break;
            }

            if (health != null)
            {
                health.TakeDamage(damage);

             
                if (audioManager != null)
                {
                    audioManager.PlaySfx(audioManager.LowHealth);
                }
            }
        }

        damageCoroutine = null;
    }

    private void OnDisable()
    {
        InWater = false;

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }
}

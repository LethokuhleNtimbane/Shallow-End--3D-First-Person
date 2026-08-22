using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageInterval = 1f;

    private float damageTimer = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        HealthScript health = other.GetComponent<HealthScript>();

        if (health == null)
            return;

        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            health.TakeDamage(damage);

            Debug.Log("Monster damaged player for " + damage);

            damageTimer = damageInterval;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        damageTimer = 0f;
    }
}
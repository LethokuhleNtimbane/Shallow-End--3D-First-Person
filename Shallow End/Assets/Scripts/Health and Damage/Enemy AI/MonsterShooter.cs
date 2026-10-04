using UnityEngine;

public class MonsterShooter : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask destroyOnHitLayers;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider other)
    {
        if (other == null)
            return;


        if (other.CompareTag("Player"))
        {
            HealthScript health =
                other.GetComponent<HealthScript>();

            if (health == null)
                health = other.GetComponentInParent<HealthScript>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }


        if (((1 << other.gameObject.layer) & destroyOnHitLayers) != 0)
        {
            Destroy(gameObject);
        }
    }
}
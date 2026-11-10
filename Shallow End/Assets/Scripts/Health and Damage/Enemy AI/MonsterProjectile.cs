using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 direction;
    private Transform owner;

    public void SetDirection(Vector3 newDirection, Transform projectileOwner)
    {
        direction = newDirection.normalized;
        owner = projectileOwner;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignores the monster that fired the projectile cause i know this gon cause problems later
        if (owner != null &&
            (other.transform == owner ||
             other.transform.IsChildOf(owner)))
        {
            return;
        }

        // Destroys when hitting the player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Monster projectile hit the player!");
            Destroy(gameObject);
            return;
        }

        // Destroyed when hits a Rigidbody
        if (other.attachedRigidbody != null)
        {
            Destroy(gameObject);
            return;
        }

        // Destroyed when hitting any collider
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}


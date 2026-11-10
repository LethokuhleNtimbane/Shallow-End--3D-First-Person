using UnityEngine;

public class ProjectileMonsterMovement : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private float detectionRange = 20f;
    private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Combat Distance")]
    [SerializeField] private float preferredDistance = 10f;
    [SerializeField] private float minimumDistance = 6f;

    [Header("Projectile Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootingRange = 15f;
    [SerializeField] private float shootingCooldown = 2f;

    private float nextShotTime;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
        else
            Debug.LogError("No Player tagged object found!");
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector3 direction = player.position - rb.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > detectionRange) return;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();

            // Face the player always when i range 
            Quaternion targetRotation =
     Quaternion.LookRotation(direction) *
     Quaternion.Euler(0f, 180f, 0f);

            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                )
            );

            float movementAmount = 0f;

            if (distance > preferredDistance)
            {
                movementAmount = 1f;
            }
            else if (distance < minimumDistance)
            {
                movementAmount = -1f;
            }

            Vector3 movement =
                direction * movementAmount * moveSpeed *
                Time.fixedDeltaTime;

            rb.MovePosition(rb.position + movement);

            // Shoot at da player when within range
            if (distance <= shootingRange &&
                distance >= minimumDistance &&
                distance <= detectionRange)
            {
                Shoot();
            }
        }
    }


    private void Shoot()
    {
        if (Time.time < nextShotTime) return;

        if (projectilePrefab == null || firePoint == null)
            return;

        Vector3 targetPosition = player.position + Vector3.up * 1.1f;

        Vector3 direction = targetPosition - firePoint.position;
        direction.Normalize();

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        MonsterProjectile projectileScript =
            projectile.GetComponent<MonsterProjectile>();

        if (projectileScript != null)
        {
            projectileScript.SetDirection(direction, transform);
        }

        nextShotTime = Time.time + shootingCooldown;
    }
}


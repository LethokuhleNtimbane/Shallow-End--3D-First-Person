using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private HealthScript playerHealth;
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private float chargeTimer;

    private bool isCharging;
    private bool hasExploded;
    private bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;


        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }


        if (player != null && playerHealth == null)
        {
            playerHealth =
                player.GetComponent<HealthScript>();
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
            return;

        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                player.position
            );


        if (distanceToPlayer > detectionRange)
        {
            return;
        }

   
        if (hasExploded)
        {
            return;
        }

    
        if (isCharging)
        {
            HandleCharge();
            return;
        }


        if (distanceToPlayer <= attackDistance)
        {
            StartCharge();
            return;
        }

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position - transform.position;


        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();


        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

  
        transform.position +=
            direction * moveSpeed * Time.deltaTime;
    }

    private void StartCharge()
    {
        if (isCharging)
            return;

        if (hasExploded)
            return;

        isCharging = true;
        chargeTimer = 0f;

        
    }

    private void HandleCharge()
    {
        chargeTimer += Time.deltaTime;


        if (player != null)
        {
            Vector3 direction =
                player.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }

        if (chargeTimer >= chargeTime)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (isDead)
            return;

        if (hasExploded)
            return;

        hasExploded = true;
        isCharging = false;
        chargeTimer = 0f;

 

        if (explosionParticle != null)
        {
            explosionParticle.Play();
        }

        if (player == null || playerHealth == null)
            return;

        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distanceToPlayer <= explosionRadius)
        {
            playerHealth.TakeDamage(explosionDamage);

        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

       

        Destroy(gameObject);
    }

    public void RemoveFromFireProtection()
    {
        if (isDead)
            return;

       

        Die();
    }
}
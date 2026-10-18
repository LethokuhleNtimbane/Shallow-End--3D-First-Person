using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    [SerializeField] private Transform Player;

    private bool playerIsSleeping = false;

    [SerializeField] private TimeController timeController;
    [SerializeField] private float monsterStartHour = 21f;
    [SerializeField] private float monsterDisappearHour = 5f;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float patrolWaitTime = 2f;

    private Vector3 patrolCenter;
    private Vector3 patrolTarget;
    private float patrolWaitTimer;
    private bool waitingAtPatrolPoint;

    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private GameObject monsterVisual;

    [SerializeField] private float explosionTime = 3f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private ParticleSystem explosionParticle;

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBar;
    [SerializeField] private float knifeDamage;

    private float currentHealth;

    private bool monsterAwake;
    private bool playerIsProtected;
    private bool isStunned;
    private bool isDead;

    private bool isCountingDown = false;
    private float explosionTimer = 0f;

    private Coroutine stunCoroutine;

    public static bool IsDialoguePaused { get; private set; }

    public static void SetDialoguePaused(bool paused)
    {
        IsDialoguePaused = paused;
    }

    private void Start()
    {
        if (timeController == null)
            timeController = TimeController.instance;

        currentHealth = maxHealth;

        patrolCenter = transform.position;
        ChooseNewPatrolTarget();

        UpdateHealthBar();

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        UpdateMonster();
    }

    private void Update()
    {
        if (IsDialoguePaused)
        {
            ResetExplosionTimer();
            return;
        }

        if (playerIsSleeping)
            return;

        if (timeController == null)
            return;

        if (isDead)
            return;

        UpdateMonster();

        if (!monsterAwake)
            return;

        if (playerIsProtected)
        {
            ResetExplosionTimer();
            return;
        }

        if (isStunned)
        {
            ResetExplosionTimer();
            return;
        }

        FollowPlayer();
    }

    private void UpdateMonster()
    {
        if (isDead)
            return;

        if (timeController == null)
            return;

        float currentHour =
            (float)timeController.CurrentTime.TimeOfDay.TotalHours;

        bool shouldMonsterBeActive =
            currentHour >= monsterStartHour ||
            currentHour < monsterDisappearHour;

        if (shouldMonsterBeActive != monsterAwake)
        {
            monsterAwake = shouldMonsterBeActive;

            if (monsterAwake)
            {
                ShowMonster();
            }
            else
            {
                ResetExplosionTimer();
                HideMonster();
            }
        }
    }

    private void FollowPlayer()
    {
        if (Player == null)
        {
            Patrol();
            return;
        }

        Vector3 direction = Player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // Player is too far away then patrol instead.
        if (distance > detectionRange)
        {
            ResetExplosionTimer();
            Patrol();
            return;
        }

        // Player is close enough to attack.
        if (distance <= attackDistance)
        {
            StartExplosionTimer();
            return;
        }

        ResetExplosionTimer();

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Patrol()
    {
        if (waitingAtPatrolPoint)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                waitingAtPatrolPoint = false;
                patrolWaitTimer = 0f;
                ChooseNewPatrolTarget();
            }

            return;
        }

        Vector3 direction = patrolTarget - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= 0.5f)
        {
            waitingAtPatrolPoint = true;
            patrolWaitTimer = 0f;
            return;
        }

        direction.Normalize();

        transform.position += direction * patrolSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ChooseNewPatrolTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;

        patrolTarget = patrolCenter +
            new Vector3(randomPoint.x, 0f, randomPoint.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;

        Vector3 center = Application.isPlaying
            ? patrolCenter
            : transform.position;

        Gizmos.DrawWireSphere(center, patrolRadius);
    }

    private void StartExplosionTimer()
    {
        if (isCountingDown)
        {
            explosionTimer += Time.deltaTime;

            if (explosionTimer >= explosionTime)
                Explode();

            return;
        }

        isCountingDown = true;
        explosionTimer = 0f;
    }

    private void ResetExplosionTimer()
    {
        isCountingDown = false;
        explosionTimer = 0f;
    }

    private void Explode()
    {
        if (isDead)
            return;

        isCountingDown = false;
        explosionTimer = 0f;

        if (explosionParticle != null)
            explosionParticle.Play();

        if (Player != null)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    Player.position
                );

            if (distance <= explosionRadius)
            {
                HealthScript playerHealth =
                    Player.GetComponent<HealthScript>();

                if (playerHealth != null)
                    playerHealth.TakeDamage(explosionDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        UpdateHealthBar();

        if (currentHealth <= 0f)
            Die();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        if (maxHealth <= 0f)
            return;

        healthBar.fillAmount =
            currentHealth / maxHealth;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        monsterAwake = false;

        ResetExplosionTimer();

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        isStunned = false;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (monsterVisual != null)
            monsterVisual.SetActive(false);

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    public void Stun(float duration)
    {
        if (isDead)
            return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        ResetExplosionTimer();

        stunCoroutine =
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        yield return new WaitForSeconds(duration);

        if (isDead)
            yield break;

        isStunned = false;

        if (monsterAwake &&
            !playerIsProtected &&
            !IsDialoguePaused)
        {
            if (attackHitbox != null)
                attackHitbox.SetActive(true);
        }

        stunCoroutine = null;
    }

    public void SetPlayerProtected(bool protectedByFire)
    {
        if (isDead)
            return;

        playerIsProtected = protectedByFire;

        if (playerIsProtected)
        {
            ResetExplosionTimer();
            HideMonster();
        }
        else
        {
            if (monsterAwake &&
                !IsDialoguePaused)
            {
                ShowMonster();
            }
        }
    }

    public void SetPlayerSleeping(bool sleeping)
    {
        if (isDead)
            return;

        playerIsSleeping = sleeping;

        if (playerIsSleeping)
        {
            ResetExplosionTimer();

            if (monsterVisual != null)
                monsterVisual.SetActive(false);

            if (attackHitbox != null)
                attackHitbox.SetActive(false);
        }
        else
        {
            if (monsterAwake &&
                !playerIsProtected &&
                !IsDialoguePaused)
            {
                ShowMonster();
            }
        }
    }

    private void ShowMonster()
    {
        if (isDead)
            return;

        if (IsDialoguePaused)
            return;

        if (monsterVisual != null)
            monsterVisual.SetActive(true);

        if (attackHitbox != null &&
            !isStunned)
        {
            attackHitbox.SetActive(true);
        }

        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
            UpdateHealthBar();
        }
    }

    private void HideMonster()
    {
        if (monsterVisual != null)
            monsterVisual.SetActive(false);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ProtectionZone"))
            SetPlayerProtected(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ProtectionZone"))
            SetPlayerProtected(false);
    }
}
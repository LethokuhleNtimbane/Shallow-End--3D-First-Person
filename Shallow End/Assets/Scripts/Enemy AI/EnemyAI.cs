using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy")]
    public float EnemyHealth = 100f;

    [SerializeField] private Image healthFill;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Wandering")]
    public float minMoveTime = 1f;
    public float maxMoveTime = 4f;
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 2f;

    private Vector3 moveDirection;
    private float timer;
    private bool moving;

    [Header("Player Detection")]
    public LayerMask whatIsPlayer;
    public float sightRange = 15f;
    public float AttackRange = 5f;

    public bool playerInSightRange;
    public bool playerInAttackRange;

    [Header("Attack")]
    public GameObject projectile;
    public float timeBetweenAttacks = 2f;

    private bool alreadyAttacked;

    public Transform player;

    [Header("Knife Attack")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionReference knifeAttackAction;
    [SerializeField] private float knifeAttackRange = 4f;
    [SerializeField] private int knifeDamage = 20;

    private bool enemyDead;

    private void Awake()
    {
        GameObject playerObject =
            GameObject.Find("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
        else
        {
          
        }

        if (inventory == null &&
            playerObject != null)
        {
            inventory =
                playerObject.GetComponent<Inventory>();
        }

        if (playerCamera == null &&
            playerObject != null)
        {
            playerCamera =
                playerObject.GetComponentInChildren<Camera>();
        }
    }

    private void Start()
    {
        ChooseNewDirection();

        UpdateHealthUI();
    }

    private void OnEnable()
    {
        if (knifeAttackAction != null)
        {
            knifeAttackAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (knifeAttackAction != null)
        {
            knifeAttackAction.action.Disable();
        }
    }

    private void Update()
    {
        if (enemyDead)
            return;

        HandleKnifeAttack();

        if (player == null)
            return;

        playerInSightRange =
            Physics.CheckSphere(
                transform.position,
                sightRange,
                whatIsPlayer
            );

        playerInAttackRange =
            Physics.CheckSphere(
                transform.position,
                AttackRange,
                whatIsPlayer
            );

        if (!playerInSightRange &&
            !playerInAttackRange)
        {
            Wander();
        }

        if (playerInSightRange &&
            !playerInAttackRange)
        {
            ChasePlayer();
        }

        if (playerInSightRange &&
            playerInAttackRange)
        {
            AttackPlayer();
        }
    }

    private void HandleKnifeAttack()
    {
        if (knifeAttackAction == null)
            return;

        if (!knifeAttackAction.action.WasPressedThisFrame())
            return;

        if (inventory == null)
            return;

        if (!inventory.IsKnifeEquipped())
            return;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            knifeAttackRange))
        {
            return;
        }

        EnemyAI enemy =
            hit.collider.GetComponentInParent<EnemyAI>();

        if (enemy == null)
            return;

        enemy.TakeDamage(knifeDamage);

       
        inventory.UseEquippedDurability();
    }

    private void Wander()
    {
        timer -= Time.deltaTime;

        if (moving)
        {
            transform.position +=
                moveDirection *
                moveSpeed *
                Time.deltaTime;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        moveDirection,
                        Vector3.up
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed *
                        Time.deltaTime
                    );
            }

            if (timer <= 0f)
            {
                moving = false;

                timer =
                    Random.Range(
                        minWaitTime,
                        maxWaitTime
                    );
            }
        }
        else
        {
            if (timer <= 0f)
            {
                ChooseNewDirection();
            }
        }
    }

    private void ChooseNewDirection()
    {
        moveDirection =
            new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

        moving = true;

        timer =
            Random.Range(
                minMoveTime,
                maxMoveTime
            );
    }

    private void ChasePlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        direction.Normalize();

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }
    }

    private void AttackPlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        if (!alreadyAttacked)
        {
            GameObject newProjectile =
                Instantiate(
                    projectile,
                    transform.position,
                    Quaternion.identity
                );

            Rigidbody rb =
                newProjectile
                    .GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(
                    transform.forward *
                    32f,
                    ForceMode.Impulse
                );

                rb.AddForce(
                    transform.up *
                    8f,
                    ForceMode.Impulse
                );
            }

            alreadyAttacked = true;

            Invoke(
                nameof(ResetAttack),
                timeBetweenAttacks
            );
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        if (enemyDead)
            return;

        EnemyHealth -= damage;

        if (EnemyHealth < 0f)
        {
            EnemyHealth = 0f;
        }

        UpdateHealthUI();

        if (EnemyHealth <= 0f)
        {
            enemyDead = true;

            CancelInvoke(
                nameof(ResetAttack)
            );

            Invoke(
                nameof(DestroyEnemy),
                0.5f
            );
        }
    }

    private void UpdateHealthUI()
    {
        if (healthFill == null)
            return;

        if (EnemyHealth <= 0f)
        {
            healthFill.fillAmount = 0f;
            return;
        }

        healthFill.fillAmount =
            EnemyHealth / 100f;
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            AttackRange
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(
            transform.position,
            knifeAttackRange
        );
    }
}
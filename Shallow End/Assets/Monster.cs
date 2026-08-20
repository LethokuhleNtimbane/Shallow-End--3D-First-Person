using UnityEngine;

public class Monster : MonoBehaviour
{

    private float damageTimer = 0f;

    [SerializeField] private float damageInterval = 1f;

    [SerializeField] private Transform Player;
    [SerializeField] private PlayerController controller;
    [SerializeField] private TimeController timeController;
    [SerializeField] private HealthScript healthScript;

    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float moveSpeed = 2f;
    bool shouldBeActive;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float monsterStartHour = 21f;
    [SerializeField] private float monsterDisappearHour = 5f;

    private bool monsterAwake = false;

    private void Start()
    {
       UpdateMonster();
    }

    private void Update()
    {
       UpdateMonster();

        if (!monsterAwake)
            return;

        FollowPlayer();
    }

    private void UpdateMonster()
    {
        float currentHour = (float)timeController.CurrentTime.TimeOfDay.TotalHours;
        if (currentHour >= monsterStartHour)
        {
            shouldBeActive = true;
        }
        
        else if (currentHour < monsterDisappearHour)
        {
            shouldBeActive = true;
        }
        else
        {
            shouldBeActive = false;
        }

        if (shouldBeActive != monsterAwake)
        {
            monsterAwake = shouldBeActive;

            gameObject.SetActive(monsterAwake);
        }
    }

    private void FollowPlayer()
    {
        if (Player == null)
            return;

        Vector3 direction = Player.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= attackDistance)
            return;

        direction.Normalize();

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!monsterAwake)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (healthScript == null)
            return;

        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            healthScript.TakeDamage(damage);

            Debug.Log("Monster damaged the player for " + damage);

            damageTimer = damageInterval;
        }
    }
}
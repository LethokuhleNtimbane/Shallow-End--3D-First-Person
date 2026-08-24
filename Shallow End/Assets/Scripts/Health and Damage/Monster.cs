using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour
{

    [SerializeField] private Transform Player;
    private bool playerIsSleeping = false;

    [SerializeField] private TimeController timeController;
    [SerializeField] private float monsterStartHour = 21f;
    [SerializeField] private float monsterDisappearHour = 5f;


    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private GameObject attackHitbox;


    [SerializeField] private GameObject monsterVisual;

    private bool shouldBeActive;
    private bool monsterAwake;
    private bool playerIsProtected;
    private bool isStunned;
 

    private Coroutine stunCoroutine;

    private void Start()
    {
        UpdateMonster();
    }
    public void SetPlayerSleeping(bool sleeping)
    {
        playerIsSleeping = sleeping;

        if (sleeping)
        {
        
            monsterAwake = false;

            if (monsterVisual != null)
            {
                monsterVisual.SetActive(false);
            }

            if (attackHitbox != null)
            {
                attackHitbox.SetActive(false);
            }
        }
        else
        {
            
            UpdateMonster();
        }
    }

    private void Update()
    {
        if (playerIsSleeping)
            return;
        if (timeController == null)
            return;

        UpdateMonster();

        if (!monsterAwake)
            return;

     
        if (playerIsProtected)
            return;

    
        if (isStunned)
            return;

        FollowPlayer();
    }

    private void UpdateMonster()
    {
        float currentHour =
            (float)timeController.CurrentTime.TimeOfDay.TotalHours;

        if (currentHour >= monsterStartHour ||
            currentHour < monsterDisappearHour)
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

            if (monsterAwake)
            {
                ShowMonster();
            }
            else
            {
                HideMonster();
            }
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
        {
            return;
        }

        direction.Normalize();

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }



    public void Stun(float duration)
    {
  

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        yield return new WaitForSeconds(duration);

        isStunned = false;


        if (monsterAwake && !playerIsProtected)
        {
            if (attackHitbox != null)
            {
                attackHitbox.SetActive(true);
            }
        }

        stunCoroutine = null;
    }



    public void SetPlayerProtected(bool protectedByFire)
    {
        playerIsProtected = protectedByFire;

        if (playerIsProtected)
        {
            HideMonster();
        }
        else
        {
            if (monsterAwake)
            {
                ShowMonster();
            }
        }
    }



    private void ShowMonster()
    {
        if (monsterVisual != null)
            monsterVisual.SetActive(true);

        if (attackHitbox != null && !isStunned)
            attackHitbox.SetActive(true);
    }

    private void HideMonster()
    {
        if (monsterVisual != null)
            monsterVisual.SetActive(false);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }
}
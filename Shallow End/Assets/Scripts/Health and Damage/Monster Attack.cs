using UnityEngine;
using TMPro;
using System.Collections;

public class MonsterAttack : MonoBehaviour
{
 
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageInterval = 1f;

    
    [SerializeField] private TextMeshProUGUI monsterRunText;
    [SerializeField] private float messageDuration = 1f;

    private float damageTimer = 0f;
    private Coroutine messageCoroutine;

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

      

            ShowMonsterMessage();

            damageTimer = damageInterval;
        }
    }

    private void ShowMonsterMessage()
    {
        if (monsterRunText == null)
            return;

        monsterRunText.text = "Monster, quickly start a fire";
        monsterRunText.gameObject.SetActive(true);

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(HideMonsterMessage());
    }

    private IEnumerator HideMonsterMessage()
    {
        yield return new WaitForSeconds(messageDuration);

        if (monsterRunText != null)
        {
            monsterRunText.gameObject.SetActive(false);
        }

        messageCoroutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        damageTimer = 0f;
    }
}
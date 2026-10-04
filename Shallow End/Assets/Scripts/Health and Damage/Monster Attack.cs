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

    AudioManager audioManager;

    public static bool IsDialoguePaused { get; private set; }

    public static void SetDialoguePaused(bool paused)
    {
        IsDialoguePaused = paused;
    }

    private void Awake()
    {
        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
        {
            audioManager =
                audioObject.GetComponent<AudioManager>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsDialoguePaused)
            return;

        if (!other.CompareTag("Player"))
            return;

        HealthScript health =
            other.GetComponent<HealthScript>();

        if (health == null)
            return;

        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            health.TakeDamage(damage);

            if (audioManager != null)
            {
                audioManager.PlaySfx(audioManager.LowHealth);
            }

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

        messageCoroutine =
            StartCoroutine(HideMonsterMessage());
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
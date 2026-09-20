using UnityEngine;
using TMPro;
using System.Collections;

public class FireHurts : MonoBehaviour
{
 
    [SerializeField] private float damage = 10f;

 
    [SerializeField] private TextMeshProUGUI ouchText;
    [SerializeField] private float messageDuration = 1f;
    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private Coroutine messageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        HealthScript health = other.GetComponent<HealthScript>();

        if (health == null)
        {
            return;
        }

        health.TakeDamage(damage);
        audioManager.PlaySfx(audioManager.LowHealth);
        ShowOuch();
    }

    private void ShowOuch()
    {
        if (ouchText == null)
            return;

        ouchText.text = "Ouch";
        ouchText.gameObject.SetActive(true);

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(HideOuch());
    }

    private IEnumerator HideOuch()
    {
        yield return new WaitForSeconds(messageDuration);

        if (ouchText != null)
        {
            ouchText.gameObject.SetActive(false);
        }

        messageCoroutine = null;
    }
}
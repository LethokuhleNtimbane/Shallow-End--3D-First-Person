using System.Collections;
using UnityEngine;

public class RespawnMR : MonoBehaviour
{
    [SerializeField] private float respawnTime = 30f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Collider[] resourceColliders;
    private Renderer[] renderers;
    private Rigidbody resourceRigidbody;

    private bool respawning = false;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        resourceColliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
        resourceRigidbody = GetComponent<Rigidbody>();
    }

    public void RespawnResource()
    {
        if (!respawning)
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        respawning = true;

        SetResourceVisible(false);

        if (resourceRigidbody != null)
        {
            resourceRigidbody.linearVelocity = Vector3.zero;
            resourceRigidbody.angularVelocity = Vector3.zero;
            resourceRigidbody.isKinematic = true;
        }

        yield return new WaitForSeconds(respawnTime);

        transform.SetPositionAndRotation(originalPosition, originalRotation);
        transform.localScale = originalScale;

        if (resourceRigidbody != null)
        {
            resourceRigidbody.linearVelocity = Vector3.zero;
            resourceRigidbody.angularVelocity = Vector3.zero;
            resourceRigidbody.isKinematic = false;
        }

        SetResourceVisible(true);

        respawning = false;
    }

    private void SetResourceVisible(bool visible)
    {
        foreach (Collider resourceCollider in resourceColliders)
        {
            if (resourceCollider != null)
                resourceCollider.enabled = visible;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }
}
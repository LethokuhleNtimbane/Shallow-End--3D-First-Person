using System.Collections;
using UnityEngine;

public class ResourceRespawn : MonoBehaviour
{
   


    [SerializeField] private float respawnTime = 30f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Collider resourceCollider;
    private Renderer[] renderers;

   

    private bool respawning = false;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        resourceCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
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

       

        // Hide the resource
        SetResourceVisible(false);

     

        // Wait for respawn time
        yield return new WaitForSeconds(respawnTime);

  

        // Restore position
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Show resource
        SetResourceVisible(true);

       

        respawning = false;
    }

    private void SetResourceVisible(bool visible)
    {
        // Turn collider on/off
        if (resourceCollider != null)
        {
            resourceCollider.enabled = visible;
        }

        // Turn mesh/renderers on/off
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }
}

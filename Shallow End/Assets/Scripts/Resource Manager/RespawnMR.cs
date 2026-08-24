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

       


        SetResourceVisible(false);

     


        yield return new WaitForSeconds(respawnTime);

  

    
        transform.position = originalPosition;
        transform.rotation = originalRotation;

 
        SetResourceVisible(true);

       

        respawning = false;
    }

    private void SetResourceVisible(bool visible)
    {
       
        if (resourceCollider != null)
        {
            resourceCollider.enabled = visible;
        }


        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }
}

using UnityEngine;

public class Destroycrab : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crab"))
        {
            Destroy(other.gameObject);
        }
    }
}
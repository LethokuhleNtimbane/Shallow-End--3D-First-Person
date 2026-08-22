using UnityEngine;

public class FireProtect : MonoBehaviour
{
    [SerializeField] private Monster monster;

    private bool fireIsActive = false;

    public void SetFire(bool active)
    {
        fireIsActive = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!fireIsActive)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (monster != null)
        {
            monster.SetPlayerProtected(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (monster != null)
        {
            monster.SetPlayerProtected(false);
        }
    }
}
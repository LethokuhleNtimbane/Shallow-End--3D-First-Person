using UnityEngine;

public class FireProtect : MonoBehaviour
{
    private bool fireIsActive = false;
    private bool playerInside = false;

    public void SetFire(bool active)
    {
        fireIsActive = active;

        if (!fireIsActive)
        {
            SetAllMonstersProtected(false);
            return;
        }

        if (playerInside)
        {
            SetAllMonstersProtected(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (!fireIsActive)
            return;

        SetAllMonstersProtected(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        SetAllMonstersProtected(false);
    }

    private void SetAllMonstersProtected(bool protectedState)
    {
        Monster[] monsters = FindObjectsByType<Monster>(
            FindObjectsSortMode.None
        );

        foreach (Monster monster in monsters)
        {
            if (monster != null)
            {
                monster.SetPlayerProtected(protectedState);
            }
        }
    }
}
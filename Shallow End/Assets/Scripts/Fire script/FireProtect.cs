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
            SetAllMonstersActive(true);
            return;
        }

        if (playerInside)
        {
            SetAllMonstersActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (!fireIsActive)
            return;

        SetAllMonstersActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        SetAllMonstersActive(true);
    }

    private void SetAllMonstersActive(bool active)
    {
        Monster[] monsters =
            FindObjectsByType<Monster>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Monster monster in monsters)
        {
            if (monster != null)
            {
                monster.gameObject.SetActive(active);
            }
        }
    }
}
using UnityEngine;

public class GroundItemManager : MonoBehaviour
{
    public static GroundItemManager Instance;

    [System.Serializable]
    public class ItemLimit
    {
        public GameObject prefab;
        public int maximumAmount = 10;
    }

    [SerializeField] private ItemLimit[] itemLimits;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanSpawn(GameObject prefab)
    {
        foreach (ItemLimit limit in itemLimits)
        {
            if (limit.prefab == prefab)
            {
                int currentAmount = CountObjectsOfPrefab(prefab);

              


                if (currentAmount >= limit.maximumAmount)
                {
               
                

                    return false;
                }

                return true;
            }
        }

      
        return true;
    }

    private int CountObjectsOfPrefab(GameObject prefab)
    {
        int count = 0;

        GroundItem[] groundItems =
            FindObjectsByType<GroundItem>(FindObjectsSortMode.None);

        foreach (GroundItem groundItem in groundItems)
        {
            if (groundItem.sourcePrefab == prefab)
            {
                count++;
            }
        }

        return count;
    }
}
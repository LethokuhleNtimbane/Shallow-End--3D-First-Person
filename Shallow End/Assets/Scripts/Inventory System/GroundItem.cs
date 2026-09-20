using UnityEngine;

public class GroundItem : MonoBehaviour
{
    public Items item;
    public int amount;

    [Header("Individual Item Durability")]
    public int currentDurability = -1;

    public GameObject sourcePrefab;
}
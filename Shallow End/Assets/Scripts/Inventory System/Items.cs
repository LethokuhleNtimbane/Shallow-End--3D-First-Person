using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]
public class Items : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack;
    public GameObject ItenPrefab;

    public GameObject handItem;

    public bool isFood;
    public float healthRegenerate;

    [Header("Durability")]
    public bool hasDurability;
    public bool infiniteDurability;
    public int maxDurability = 1;
}
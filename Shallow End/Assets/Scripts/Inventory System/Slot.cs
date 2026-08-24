using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private Items heldItem;

    private int ItemAmount;

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountTxt;
    [SerializeField] private GameObject selectionFrame;


    public void SelectedFrame(bool selected)
    {
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(selected);

        }
    }

    public Items GetItem()
    {
        return heldItem;
    }
    public int GetAmount()
    {
        return ItemAmount;
    }
    public void SetItem(Items item, int amount = 1)
    {
        heldItem = item;
        ItemAmount = amount;

        UpdateSlot();
    }
    public void UpdateSlot()
    {
        if (iconImage == null)
        {
            iconImage = transform.GetChild(0).GetComponent<Image>();
            amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }
        if (heldItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountTxt.text = ItemAmount.ToString();
        }
        else
        {
            iconImage.enabled = false;
            amountTxt.text = "";
        }
    }
    public int AddAmount(int amountToAdd)
    {
        ItemAmount += amountToAdd;
        UpdateSlot();
        return ItemAmount;
    }
    public int RemoveAmount(int amountToRemove)
    {
        ItemAmount -= amountToRemove;
        if (ItemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return ItemAmount;
    }
    public void ClearSlot()
    {
        heldItem = null;
        ItemAmount = 0;
        UpdateSlot();
    }
    public bool Hasitem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

      
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private Items heldItem;
    private int ItemAmount;

    // Current durability of THIS individual item.
    private int currentDurability;

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountTxt;
    [SerializeField] private GameObject selectionFrame;

    [Header("Durability UI")]
    [SerializeField] private Image durabilityBackground;
    [SerializeField] private Image durabilityFill;

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

    public int GetDurability()
    {
        return currentDurability;
    }

    public void SetItem(
        Items item,
        int amount = 1,
        int durability = -1)
    {
        heldItem = item;
        ItemAmount = amount;

        if (heldItem != null &&
            heldItem.hasDurability &&
            !heldItem.infiniteDurability)
        {
            if (durability < 0)
            {
                currentDurability = heldItem.maxDurability;
            }
            else
            {
                currentDurability =
                    Mathf.Clamp(
                        durability,
                        0,
                        heldItem.maxDurability
                    );
            }
        }
        else
        {
            currentDurability = 0;
        }

        UpdateSlot();
    }

    public void SetDurability(int durability)
    {
        if (heldItem == null)
            return;

        if (!heldItem.hasDurability ||
            heldItem.infiniteDurability)
            return;

        currentDurability =
            Mathf.Clamp(
                durability,
                0,
                heldItem.maxDurability
            );

        UpdateDurabilityUI();
    }

    public bool UseDurability(int amount = 1)
    {
        if (heldItem == null)
            return false;

        // Items with no durability don't lose durability.
        if (!heldItem.hasDurability)
            return false;

        // Axe is infinite.
        if (heldItem.infiniteDurability)
            return false;

        currentDurability -= amount;

        if (currentDurability <= 0)
        {
            currentDurability = 0;

            ClearSlot();

            return true;
        }

        UpdateDurabilityUI();

        return false;
    }

    public void UpdateSlot()
    {
        if (iconImage == null)
        {
            if (transform.childCount > 0)
            {
                iconImage =
                    transform.GetChild(0).GetComponent<Image>();
            }
        }

        if (amountTxt == null)
        {
            if (transform.childCount > 1)
            {
                amountTxt =
                    transform.GetChild(1)
                        .GetComponent<TextMeshProUGUI>();
            }
        }

        if (heldItem != null)
        {
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = heldItem.icon;
            }

            if (amountTxt != null)
            {
                amountTxt.text =
                    ItemAmount.ToString();
            }
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (amountTxt != null)
            {
                amountTxt.text = "";
            }
        }

        UpdateDurabilityUI();
    }

    private void UpdateDurabilityUI()
    {
        if (durabilityBackground == null ||
            durabilityFill == null)
        {
            return;
        }

        // Empty slot = no durability UI.
        if (heldItem == null)
        {
            durabilityBackground.gameObject.SetActive(false);
            durabilityFill.gameObject.SetActive(false);

            return;
        }

        // Items that don't have durability don't need the bar.
        if (!heldItem.hasDurability)
        {
            durabilityBackground.gameObject.SetActive(false);
            durabilityFill.gameObject.SetActive(false);

            return;
        }

        durabilityBackground.gameObject.SetActive(true);
        durabilityFill.gameObject.SetActive(true);

        if (heldItem.infiniteDurability)
        {
            durabilityFill.fillAmount = 1f;
            return;
        }

        if (heldItem.maxDurability <= 0)
        {
            durabilityFill.fillAmount = 0f;
            return;
        }

        durabilityFill.fillAmount =
            (float)currentDurability /
            heldItem.maxDurability;
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
        currentDurability = 0;

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
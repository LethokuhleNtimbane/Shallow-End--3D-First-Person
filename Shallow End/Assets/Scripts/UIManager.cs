using UnityEngine;

public class UIManager : MonoBehaviour
{
   public static UIManager Instance;

    public bool IsInventoryOpen;
    public bool IsCraftingOpen;

    private void Awake()
    {
        Instance = this;
    }

    public void SetInventoryOpen(bool open)
    {
        IsInventoryOpen = open;
        UpdatePlayerControl();
    }

    public void SetCraftingOpen(bool open)
    {
        IsCraftingOpen = open;
        UpdatePlayerControl();
    }

    private void UpdatePlayerControl()
    {
        bool anyUIOpen =
            IsInventoryOpen ||
            IsCraftingOpen;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CPlayerControl(!anyUIOpen);
        }
    }
}

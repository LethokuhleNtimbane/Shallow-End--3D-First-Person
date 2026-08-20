using Unity.VisualScripting;
using UnityEngine;

public class FireProtect : MonoBehaviour
{
    public bool FireOn;

    public void SetFire(bool state)
    {
        FireOn = state;
    }
}

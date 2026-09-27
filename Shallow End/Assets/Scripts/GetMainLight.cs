using UnityEngine;

[ExecuteInEditMode]
public class GetMainLight : MonoBehaviour
{
   [SerializeField] private Material SkyMaterial;

    private void Update()
    {
        SkyMaterial.SetVector("_MainLight_Direction", transform.forward);
    }
}

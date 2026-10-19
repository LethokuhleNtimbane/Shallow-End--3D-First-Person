using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TestFishingLine : MonoBehaviour
{
    private void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + Vector3.forward * 3f);
    }
}

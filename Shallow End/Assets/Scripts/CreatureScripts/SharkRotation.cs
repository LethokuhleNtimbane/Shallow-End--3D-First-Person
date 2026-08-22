using UnityEngine;

public class SharkRotation : MonoBehaviour

 
{
    [SerializeField] private float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}


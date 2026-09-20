using UnityEngine;
using UnityEngine.InputSystem;

public class Headbop : MonoBehaviour
{
    
    [Range(0.001f, 0.1f)]
    public float Amount = 0.03f;

    [Range(1f, 20f)]
    public float Frequency = 8f;

    [Range(1f, 20f)]
    public float Smooth = 10f;

    private Vector3 startPosition;
    private float bobTimer;

    private void Start()
    {

        startPosition = transform.localPosition;
    }

    private void Update()
    {
     
        Vector2 movement = GetMovementInput();

        if (movement.magnitude > 0.1f)
        {
            HeadBob();
        }
        else
        {
            ResetHeadBob();
        }
    }

    private Vector2 GetMovementInput()
    {
        Vector2 movement = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                movement.y += 1;

            if (Keyboard.current.sKey.isPressed)
                movement.y -= 1;

            if (Keyboard.current.dKey.isPressed)
                movement.x += 1;

            if (Keyboard.current.aKey.isPressed)
                movement.x -= 1;
        }

        return movement.normalized;
    }

    private void HeadBob()
    {

        bobTimer += Time.deltaTime * Frequency;

     
        float bobY = Mathf.Sin(bobTimer) * Amount;

   
        float bobX = Mathf.Cos(bobTimer * 0.5f) * Amount * 0.5f;

        Vector3 targetPosition = startPosition + new Vector3(
            bobX,
            bobY,
            0f
        );

     
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Smooth * Time.deltaTime
        );
    }

    private void ResetHeadBob()
    {
   
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            startPosition,
            Smooth * Time.deltaTime
        );


        bobTimer = 0f;
    }
}

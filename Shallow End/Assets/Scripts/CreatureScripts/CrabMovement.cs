using UnityEngine;

public class CrabMovement : MonoBehaviour
{
   
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;


    public float minMoveTime = 1f;
    public float maxMoveTime = 4f;
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 2f;

    private Vector3 moveDirection;
    private float timer;
    private bool moving;

    void Start()
    {
        ChooseNewDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (moving)
        {
            
            transform.position += moveDirection * moveSpeed * Time.deltaTime; 

           
            if (moveDirection != Vector3.zero) 
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDirection, Vector3.up);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            
            if (timer <= 0f)
            {
                moving = false;
                timer = Random.Range(minWaitTime, maxWaitTime); 
            }
        }
        else
        {
            
            if (timer <= 0f)
            {
                ChooseNewDirection(); 
            }
        }
    }

    void ChooseNewDirection()
    {
        
        moveDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        moving = true;

        timer = Random.Range(minMoveTime, maxMoveTime);
    }
}

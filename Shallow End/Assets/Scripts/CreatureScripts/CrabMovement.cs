using UnityEngine;

public class CrabMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Wandering")]
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
            
            transform.position += moveDirection * moveSpeed * Time.deltaTime; // crab moves forward for now till
            // i find a better way to make the side to side movement cause navmesh movement was not working out
           
            if (moveDirection != Vector3.zero) // rotates towards new the movement direction 
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
                timer = Random.Range(minWaitTime, maxWaitTime); // once timer ends changes to a new direction after a bit
            }
        }
        else
        {
            
            if (timer <= 0f)
            {
                ChooseNewDirection(); // waits a bit then chooses a new direction
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

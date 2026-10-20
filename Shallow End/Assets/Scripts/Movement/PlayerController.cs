
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float lookSensitivity = 2f;
    public float verticalLookLimit = 90f;

    private CharacterController Controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector2 lookInput;
    private float verticalRotation = 0f;

    public bool updateingRotation = true;

    [Header("Temporary Speed Effect")]
    private float speedMultiplier = 1f;
    private Coroutine speedBoostCoroutine;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        updateingRotation = true;
        Cursor.visible = false;

        Instance = this;
    }

    private void Start()
    {
        Controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (!updateingRotation)
        {
            moveInput = Vector2.zero;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!updateingRotation)
            return;

        if (context.performed && Controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void CPlayerControl(bool inControl)
    {
        updateingRotation = inControl;

        if (!inControl)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void PlayerControl(bool PlayercanControl)
    {
        if (!PlayercanControl)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
        }

        updateingRotation = PlayercanControl;
    }

    // Called by FoodScript when the mushroom grants faster movement.
    public void ApplyTemporarySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }

        speedBoostCoroutine = StartCoroutine(
            SpeedBoostRoutine(multiplier, duration)
        );
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        speedMultiplier = Mathf.Max(1f, multiplier);

        yield return new WaitForSeconds(duration);

        speedMultiplier = 1f;
        speedBoostCoroutine = null;
    }

    public void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        verticalRotation -= mouseY;

        verticalRotation = Mathf.Clamp(
            verticalRotation,
            -verticalLookLimit,
            verticalLookLimit
        );

        cameraTransform.localRotation = Quaternion.Euler(
            verticalRotation, 0f, 0f
        );

        transform.Rotate(Vector3.up * mouseX);
    }

    private void Update()
    {
        if (updateingRotation)
        {
            Vector3 move =
                transform.right * moveInput.x +
                transform.forward * moveInput.y;

            Controller.Move(
                move * speed * speedMultiplier * Time.deltaTime
            );

            HandleLook();
        }

        velocity.y += gravity * Time.deltaTime;
        Controller.Move(velocity * Time.deltaTime);
    }
}

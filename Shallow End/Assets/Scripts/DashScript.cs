using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DashScript : MonoBehaviour
{
    
    // dash settings 
    [SerializeField] private float dashDistance = 6f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    // dash direction 
    [SerializeField] private bool allowAirDash = true;// can dash and jump at the same time just so theres no
                                                      // issues when accidentally doing both but we could remove this if we choose to remove the jump mehanic 
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float dashStaminaCost = 25f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1f;

    [SerializeField] private Image staminaBar;
    [SerializeField] private Image background;
    private CharacterController controller;
    private PlayerController playerController;

    private bool isDashing;
    private float cooldownTimer;

    private float currentStamina;
    private float staminaRegenTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();

        currentStamina = maxStamina;

        UpdateStaminaBar();
    }

    private void Update()
    {

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

       
        if (currentStamina < maxStamina)
        {
            staminaRegenTimer -= Time.deltaTime;

            if (staminaRegenTimer <= 0f)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;

                currentStamina = Mathf.Clamp(
                    currentStamina,
                    0f,
                    maxStamina
                );

                UpdateStaminaBar();
            }
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (isDashing)
            return;

        if (cooldownTimer > 0f)
            return;

        if (!allowAirDash && !controller.isGrounded)
            return;

      
        if (currentStamina < dashStaminaCost)
            return;

       
        currentStamina -= dashStaminaCost;

       
        staminaRegenTimer = staminaRegenDelay;

        UpdateStaminaBar();

        StartCoroutine(PerformDash());
    }

    private System.Collections.IEnumerator PerformDash()
    {
        isDashing = true;
        cooldownTimer = dashCooldown;

        // Gets direction from the WASD keys
        Vector2 input = Keyboard.current != null
            ? new Vector2(
                Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0
              )
            : Vector2.zero;

        Vector3 dashDirection;

        // If the player is not pressing any WASD keys then the dash goes in the direction they are facing 
        if (input.sqrMagnitude > 0.01f)
        {
            dashDirection =
                transform.right * input.x +
                transform.forward * input.y;

            dashDirection.Normalize();
        }
        else
        {
            dashDirection = transform.forward;
        }

        float dashSpeed = dashDistance / dashDuration;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        isDashing = false;
    }
    private void UpdateStaminaBar()
    {
        if (staminaBar == null)
            return;

        
        staminaBar.fillAmount = currentStamina / maxStamina;

        
        staminaBar.gameObject.SetActive(currentStamina < maxStamina);
        background.gameObject.SetActive(currentStamina < maxStamina);
    }
}
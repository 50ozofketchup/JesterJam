using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{

    [Header("Keybindings")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    
    // FEATURE TOGGLES
  
    [Header("Feature Toggles")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;

    // MOVEMENT SETTINGS

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;

    // JUMP SETTINGS

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    // How fast the player falls after jumping (higher = snappier, less floaty)
    [SerializeField] private float fallMultiplier = 2.5f;

    // CROUCH SETTINGS

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;   // CharacterController height while crouching
    [SerializeField] private float standingHeight = 2f;   // CharacterController height while standing
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchTransitionSpeed = 8f; // How fast to lerp between heights

     // GRAVITY

    [Header("Gravity")]
    // Unity's default gravity is -9.81, but bumping it feels better for games
    [SerializeField] private float gravity = -20f;

    // GROUND CHECK
 
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;

    // PRIVATE STATE

    private CharacterController _controller;
    private Vector3 _velocity;          // Tracks vertical (gravity/jump) velocity
    private bool _isGrounded;
    private bool _isCrouching;
    private float _targetHeight;        // The height we are lerping towards

    [HideInInspector] public bool inputLocked = false;
    // UNITY LIFECYCLE

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _targetHeight = standingHeight;
    }

    private void Update()
    {
        CheckGround();

        if (!inputLocked)
        {
            HandleMovement();
            if (canJump) HandleJump();
            if (canCrouch) HandleCrouch();
        }

        ApplyGravity();
        HandleCrouchTransition();
    }

    // GROUND CHECK

    private void CheckGround()
    {
        // Cast a small sphere at the groundCheck position — if it overlaps the
        // ground layer, the player is considered grounded.
        _isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask
        );

        // Reset downward velocity when grounded so we don't accumulate a huge
        // downward force while standing still.
        if (_isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f; // Small negative keeps CheckSphere reliable
        }
    }

     // MOVEMENT

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");   // W/S

        // Move relative to where the player is facing
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        // Choose speed: sprint > crouch > walk (priority order)
        float currentSpeed;
        if (canSprint && Input.GetKey(sprintKey) && !_isCrouching)
            currentSpeed = sprintSpeed;
        else if (_isCrouching)
            currentSpeed = crouchSpeed;
        else
            currentSpeed = walkSpeed;

        _controller.Move(direction.normalized * currentSpeed * Time.deltaTime);
    }

     // JUMP

    private void HandleJump()
    {
        // Prevent jumping while crouching (optional — remove the !_isCrouching
        // check below if you want to allow it)
        if (Input.GetKeyDown(jumpKey) && _isGrounded && !_isCrouching)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

     // GRAVITY

    private void ApplyGravity()
    {
        // Apply extra downward force when falling so it feels snappier
        if (_velocity.y < 0f)
            _velocity.y += gravity * (fallMultiplier - 1f) * Time.deltaTime;
        else
            _velocity.y += gravity * Time.deltaTime;

        _controller.Move(_velocity * Time.deltaTime);
    }

    // CROUCH

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            _isCrouching = true;
            _targetHeight = crouchHeight;
        }

        if (Input.GetKeyUp(crouchKey))
        {
            // Only stand up if there's nothing above the player
            if (!IsRoofAbove())
            {
                _isCrouching = false;
                _targetHeight = standingHeight;
            }
        }
    }

    private void HandleCrouchTransition()
    {
        float newHeight = Mathf.Lerp(
            _controller.height,
            _targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        // Keep the controller grounded by adjusting the center as height changes
        float heightDelta = _controller.height - newHeight;
        _controller.center = new Vector3(0f, _controller.center.y - heightDelta / 2f, 0f);
        _controller.height = newHeight;
    }
    private bool IsRoofAbove()
    {
        // Cast a ray straight up from the player's position
        return Physics.Raycast(transform.position, Vector3.up, standingHeight, groundMask);
    }

    // GIZMOS — Draws the ground check sphere in the Scene view for debugging

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
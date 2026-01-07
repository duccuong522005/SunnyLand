using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script chính điều khiển di chuyển và nhảy của player
/// Sử dụng Unity Input System và Rigidbody2D cho physics
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float coyoteTime = 0.2f; // Thời gian có thể nhảy sau khi rời mặt đất
    [SerializeField] private float jumpBufferTime = 0.2f; // Thời gian buffer khi nhấn nhảy sớm
    
    [Header("Physics Settings")]
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag = 1f;
    
    [Header("References")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private PlayerAnimation playerAnimation;
    
    // Private variables
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpInput;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    
    // Properties
    public bool IsGrounded => groundCheck != null && groundCheck.IsGrounded();
    public float MoveSpeed => moveSpeed;
    public float HorizontalInput => horizontalInput;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Tự động tìm GroundCheck nếu chưa được gán
        if (groundCheck == null)
        {
            groundCheck = GetComponent<GroundCheck>();
        }
        
        // Tự động tìm PlayerAnimation nếu chưa được gán
        if (playerAnimation == null)
        {
            playerAnimation = GetComponent<PlayerAnimation>();
        }
    }
    
    private void Update()
    {
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleSpriteFlip();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleDrag();
    }
    
    /// <summary>
    /// Xử lý di chuyển ngang của player
    /// </summary>
    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
    
    /// <summary>
    /// Xử lý nhảy của player
    /// </summary>
    private void HandleJump()
    {
        // Kiểm tra điều kiện nhảy: đang trên mặt đất hoặc trong coyote time, và có jump buffer
        if ((IsGrounded || coyoteTimeCounter > 0) && jumpBufferCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
    }
    
    /// <summary>
    /// Xử lý drag (ma sát) khi trên mặt đất và trên không
    /// </summary>
    private void HandleDrag()
    {
        if (IsGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }
    
    /// <summary>
    /// Xử lý coyote time - cho phép nhảy một chút sau khi rời mặt đất
    /// </summary>
    private void HandleCoyoteTime()
    {
        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            wasGrounded = true;
        }
        else if (wasGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (coyoteTimeCounter <= 0)
            {
                wasGrounded = false;
            }
        }
    }
    
    /// <summary>
    /// Xử lý jump buffer - cho phép nhảy nếu nhấn nhảy sớm một chút
    /// </summary>
    private void HandleJumpBuffer()
    {
        if (jumpInput)
        {
            jumpBufferCounter = jumpBufferTime;
            HandleJump();
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Xử lý flip sprite dựa trên hướng di chuyển
    /// </summary>
    private void HandleSpriteFlip()
    {
        if (playerAnimation != null && horizontalInput != 0)
        {
            playerAnimation.FlipSprite(horizontalInput);
        }
    }
    
    #region Input System Callbacks
    
    /// <summary>
    /// Callback từ Input System khi di chuyển ngang
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
    }
    
    /// <summary>
    /// Callback từ Input System khi nhấn nhảy
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpInput = true;
            jumpBufferCounter = jumpBufferTime;
        }
        else if (context.canceled)
        {
            jumpInput = false;
            
            // Cho phép nhảy ngắn hơn nếu thả sớm
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }
    
    #endregion
}


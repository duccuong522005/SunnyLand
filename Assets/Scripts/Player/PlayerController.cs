using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Core player movement and jump controller using Unity Input System and Rigidbody2D physics.
/// Implements coyote time and jump buffering for responsive platformer controls.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerController : MonoBehaviour
{
    #region Constants
    
    private const float MIN_VELOCITY_Y_THRESHOLD = 0.01f;
    private const float VARIABLE_JUMP_MULTIPLIER = 0.5f;
    
    #endregion
    
    #region Inspector Settings - Movement
    
    [Header("Movement Settings")]
    [Tooltip("Horizontal movement speed in units per second")]
    [Range(1f, 20f)]
    [SerializeField] private float _moveSpeed = 5f;
    
    [Tooltip("Vertical jump force applied when jumping")]
    [Range(5f, 30f)]
    [SerializeField] private float _jumpForce = 10f;
    
    [Tooltip("Time window after leaving ground where player can still jump (coyote time)")]
    [Range(0f, 1f)]
    [SerializeField] private float _coyoteTime = 0.2f;
    
    [Tooltip("Time window where early jump input is buffered if player isn't grounded yet")]
    [Range(0f, 1f)]
    [SerializeField] private float _jumpBufferTime = 0.2f;
    
    #endregion
    
    #region Inspector Settings - Physics
    
    [Header("Physics Settings")]
    [Tooltip("Linear damping (drag) when grounded")]
    [Range(0f, 10f)]
    [SerializeField] private float _groundDrag = 5f;
    
    [Tooltip("Linear damping (drag) when in air")]
    [Range(0f, 5f)]
    [SerializeField] private float _airDrag = 1f;
    
    #endregion
    
    #region Inspector Settings - References
    
    [Header("References")]
    [Tooltip("Ground check component for detecting ground collisions")]
    [SerializeField] private GroundCheck _groundCheck;
    
    [Tooltip("Player animation controller for sprite flipping")]
    [SerializeField] private PlayerAnimation _playerAnimation;
    
    #endregion
    
    #region Private Fields
    
    private Rigidbody2D _rb;
    private float _horizontalInput;
    private bool _jumpInput;
    private bool _wasGrounded;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    
    #endregion
    
    #region Public Properties
    
    /// <summary>
    /// Returns true if player is currently grounded
    /// </summary>
    public bool IsGrounded => _groundCheck != null && _groundCheck.IsGrounded();
    
    /// <summary>
    /// Current movement speed value
    /// </summary>
    public float MoveSpeed => _moveSpeed;
    
    /// <summary>
    /// Current horizontal input value (-1 to 1)
    /// </summary>
    public float HorizontalInput => _horizontalInput;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        CacheComponents();
        ValidateReferences();
    }
    
    private void Update()
    {
        UpdateCoyoteTime();
        ProcessJumpBuffer();
        HandleSpriteFlip();
    }
    
    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyDrag();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Cache all required components in Awake for performance
    /// </summary>
    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        if (_rb == null)
        {
            Debug.LogError($"[PlayerController] Rigidbody2D component not found on {gameObject.name}");
        }
        
        if (_groundCheck == null)
        {
            TryGetComponent(out _groundCheck);
        }
        
        if (_playerAnimation == null)
        {
            TryGetComponent(out _playerAnimation);
        }
    }
    
    /// <summary>
    /// Validate critical references and log warnings if missing
    /// </summary>
    private void ValidateReferences()
    {
        if (_groundCheck == null)
        {
            Debug.LogWarning($"[PlayerController] GroundCheck component not found on {gameObject.name}. Movement may not work correctly.");
        }
    }
    
    #endregion
    
    #region Movement
    
    /// <summary>
    /// Apply horizontal movement velocity based on input
    /// Called in FixedUpdate for physics consistency
    /// </summary>
    private void ApplyMovement()
    {
        if (_rb == null) return;
        
        Vector2 currentVelocity = _rb.linearVelocity;
        currentVelocity.x = _horizontalInput * _moveSpeed;
        _rb.linearVelocity = currentVelocity;
    }
    
    /// <summary>
    /// Apply linear damping (drag) based on grounded state
    /// </summary>
    private void ApplyDrag()
    {
        if (_rb == null) return;
        
        _rb.linearDamping = IsGrounded ? _groundDrag : _airDrag;
    }
    
    #endregion
    
    #region Jump
    
    /// <summary>
    /// Execute jump if conditions are met (grounded/coyote time + jump buffer)
    /// </summary>
    private void ExecuteJump()
    {
        bool canJump = (IsGrounded || _coyoteTimeCounter > 0f) && _jumpBufferCounter > 0f;
        
        if (!canJump || _rb == null) return;
        
        Vector2 currentVelocity = _rb.linearVelocity;
        currentVelocity.y = _jumpForce;
        _rb.linearVelocity = currentVelocity;
        
        _jumpBufferCounter = 0f;
        _coyoteTimeCounter = 0f;
    }
    
    /// <summary>
    /// Process variable jump height - reduce velocity if jump button released early
    /// </summary>
    private void ApplyVariableJumpHeight()
    {
        if (_rb == null) return;
        
        Vector2 currentVelocity = _rb.linearVelocity;
        if (currentVelocity.y > MIN_VELOCITY_Y_THRESHOLD)
        {
            currentVelocity.y *= VARIABLE_JUMP_MULTIPLIER;
            _rb.linearVelocity = currentVelocity;
        }
    }
    
    #endregion
    
    #region Coyote Time
    
    /// <summary>
    /// Update coyote time counter for forgiving jump timing
    /// </summary>
    private void UpdateCoyoteTime()
    {
        if (IsGrounded)
        {
            _coyoteTimeCounter = _coyoteTime;
            _wasGrounded = true;
        }
        else if (_wasGrounded)
        {
            _coyoteTimeCounter -= Time.deltaTime;
            if (_coyoteTimeCounter <= 0f)
            {
                _wasGrounded = false;
            }
        }
    }
    
    #endregion
    
    #region Jump Buffer
    
    /// <summary>
    /// Process jump input buffer for responsive controls
    /// </summary>
    private void ProcessJumpBuffer()
    {
        if (_jumpInput)
        {
            _jumpBufferCounter = _jumpBufferTime;
            ExecuteJump();
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    #endregion
    
    #region Visual
    
    /// <summary>
    /// Flip sprite based on movement direction
    /// </summary>
    private void HandleSpriteFlip()
    {
        if (_playerAnimation != null && Mathf.Abs(_horizontalInput) > 0.01f)
        {
            _playerAnimation.FlipSprite(_horizontalInput);
        }
    }
    
    #endregion
    
    #region Input System Callbacks
    
    /// <summary>
    /// Called by Input System when move input is received
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        _horizontalInput = context.ReadValue<Vector2>().x;
    }
    
    /// <summary>
    /// Called by Input System when jump input is received
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _jumpInput = true;
            _jumpBufferCounter = _jumpBufferTime;
        }
        else if (context.canceled)
        {
            _jumpInput = false;
            ApplyVariableJumpHeight();
        }
    }
    
    #endregion
}

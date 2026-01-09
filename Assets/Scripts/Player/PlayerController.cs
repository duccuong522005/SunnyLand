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
    
    [Tooltip("Movement speed multiplier when crouching (0 = no movement, 1 = full speed)")]
    [Range(0f, 1f)]
    [SerializeField] private float _crouchSpeedMultiplier = 0.3f;
    
    [Header("Crouch Collider Settings")]
    [Tooltip("Collider height when crouching (as percentage of normal height)")]
    [Range(0.3f, 0.9f)]
    [SerializeField] private float _crouchHeightMultiplier = 0.6f;
    
    [Tooltip("Collider offset Y when crouching (negative value moves collider down)")]
    [SerializeField] private float _crouchOffsetY = -0.3f;
    
    [Tooltip("Distance to check upward for ceiling when standing up")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _ceilingCheckDistance = 0.5f;
    
    [Tooltip("Layer mask for ceiling detection (should match ground layer)")]
    [SerializeField] private LayerMask _ceilingLayer = 1;
    
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
    
    [Tooltip("CapsuleCollider2D to adjust when crouching (auto-detected if not assigned)")]
    [SerializeField] private CapsuleCollider2D _capsuleCollider;
    
    #endregion
    
    #region Private Fields
    
    private Rigidbody2D _rb;
    private float _horizontalInput;
    private bool _jumpInput;
    private bool _crouchInput;
    private bool _isCrouching;
    private bool _wasGrounded;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    
    // Collider adjustment
    private Vector2 _originalColliderSize;
    private Vector2 _originalColliderOffset;
    
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
    
    /// <summary>
    /// Returns true if player is currently crouching
    /// </summary>
    public bool IsCrouching => _isCrouching;
    
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
        ProcessCrouchInput();
        HandleCrouchState();
        HandleSpriteFlip();
        UpdateCrouchAnimation();
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
        
        // Cache CapsuleCollider2D
        if (_capsuleCollider == null)
        {
            TryGetComponent(out _capsuleCollider);
        }
        
        if (_capsuleCollider != null)
        {
            // Store original values
            _originalColliderSize = _capsuleCollider.size;
            _originalColliderOffset = _capsuleCollider.offset;
        }
        else
        {
            Debug.LogWarning($"[PlayerController] CapsuleCollider2D not found on {gameObject.name}. Crouch collider adjustment will not work.");
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
        
        // Calculate movement speed (reduced when crouching)
        float currentMoveSpeed = _isCrouching ? _moveSpeed * _crouchSpeedMultiplier : _moveSpeed;
        
        Vector2 currentVelocity = _rb.linearVelocity;
        currentVelocity.x = _horizontalInput * currentMoveSpeed;
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
        
        // Force stop crouching when jumping
        if (_isCrouching)
        {
            _isCrouching = false;
            if (_playerAnimation != null)
            {
                _playerAnimation.SetCrouching(false);
            }
        }
        
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
    
    /// <summary>
    /// Called by Input System when crouch input is received
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnCrouch(InputAction.CallbackContext context)
    {
        // Store input state for continuous checking
        if (context.performed || context.started)
        {
            _crouchInput = true;
        }
        else if (context.canceled)
        {
            _crouchInput = false;
        }
    }
    
    #endregion
    
    #region Crouch
    
    /// <summary>
    /// Check if there's enough space above to stand up
    /// </summary>
    private bool HasSpaceAbove()
    {
        if (_capsuleCollider == null) return true;
        
        // Calculate the height difference between crouch and stand
        float heightDifference = _originalColliderSize.y - (_originalColliderSize.y * _crouchHeightMultiplier);
        
        // Check position for ceiling check (top of current crouch collider + height difference)
        Vector2 checkPosition = (Vector2)transform.position + _capsuleCollider.offset;
        float currentTop = checkPosition.y + (_capsuleCollider.size.y * 0.5f);
        Vector2 ceilingCheckPos = new Vector2(checkPosition.x, currentTop + heightDifference);
        
        // Use BoxCast upward to check for ceiling
        Vector2 boxSize = new Vector2(_capsuleCollider.size.x * 0.8f, 0.1f);
        RaycastHit2D hit = Physics2D.BoxCast(
            origin: ceilingCheckPos,
            size: boxSize,
            angle: 0f,
            direction: Vector2.up,
            distance: _ceilingCheckDistance,
            layerMask: _ceilingLayer
        );
        
        return hit.collider == null;
    }
    
    /// <summary>
    /// Check if there's a low ceiling that requires crouching
    /// This checks if standing up would cause collision with ceiling
    /// </summary>
    private bool HasLowCeiling()
    {
        if (_capsuleCollider == null) return false;
        
        // Calculate what the top position would be if standing up
        Vector2 checkPosition = (Vector2)transform.position + _originalColliderOffset;
        float standTop = checkPosition.y + (_originalColliderSize.y * 0.5f);
        
        // Check if there's a ceiling at standing height
        Vector2 boxSize = new Vector2(_originalColliderSize.x * 0.8f, 0.1f);
        Vector2 ceilingCheckPos = new Vector2(checkPosition.x, standTop);
        
        RaycastHit2D hit = Physics2D.BoxCast(
            origin: ceilingCheckPos,
            size: boxSize,
            angle: 0f,
            direction: Vector2.up,
            distance: 0.1f, // Small distance to check immediate ceiling
            layerMask: _ceilingLayer
        );
        
        // Make sure we're not detecting our own collider
        if (hit.collider != null && hit.collider != _capsuleCollider)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Adjust capsule collider size and offset when crouching
    /// </summary>
    private void AdjustColliderForCrouch()
    {
        if (_capsuleCollider == null) return;
        
        if (_isCrouching)
        {
            // Apply crouch size and offset
            Vector2 crouchSize = _originalColliderSize;
            crouchSize.y *= _crouchHeightMultiplier;
            
            Vector2 crouchOffset = _originalColliderOffset;
            crouchOffset.y += _crouchOffsetY;
            
            _capsuleCollider.size = crouchSize;
            _capsuleCollider.offset = crouchOffset;
        }
        else
        {
            // Restore original size and offset
            _capsuleCollider.size = _originalColliderSize;
            _capsuleCollider.offset = _originalColliderOffset;
        }
    }
    
    /// <summary>
    /// Process crouch input and update crouch state
    /// Called in Update to handle continuous input
    /// </summary>
    private void ProcessCrouchInput()
    {
        // If input is released, check if we can stand up before stopping crouch
        if (!_crouchInput)
        {
            // If currently crouching, check if there's a low ceiling that prevents standing up
            if (_isCrouching && HasLowCeiling() && IsGrounded)
            {
                // Keep crouching because there's a low ceiling (even though input is released)
                return;
            }
            
            // Only allow standing up if there's space above
            if (HasSpaceAbove())
            {
                _isCrouching = false;
            }
            // If no space, keep crouching (player is stuck)
            return;
        }
        
        // If input is pressed and we're grounded, set crouching
        // This will be checked every frame, so it will work even if ground check is slightly unstable
        if (_crouchInput && IsGrounded)
        {
            _isCrouching = true;
        }
    }
    
    /// <summary>
    /// Handle crouch state - automatically disable when not grounded
    /// </summary>
    private void HandleCrouchState()
    {
        // Auto-disable crouch when not grounded (player jumped or fell)
        // This ensures crouch is always disabled when in air
        if (_isCrouching && !IsGrounded)
        {
            _isCrouching = false;
            // Force update animation immediately to prevent stuck state
            if (_playerAnimation != null)
            {
                _playerAnimation.SetCrouching(false);
            }
        }
    }
    
    /// <summary>
    /// Update crouch animation state
    /// </summary>
    private void UpdateCrouchAnimation()
    {
        // Don't set crouch animation if player is in air (jump/fall takes priority)
        if (_playerAnimation != null)
        {
            if (!IsGrounded)
            {
                // If in air, ensure crouch is false
                _playerAnimation.SetCrouching(false);
            }
            else
            {
                _playerAnimation.SetCrouching(_isCrouching);
            }
        }
        
        // Adjust collider when crouch state changes
        AdjustColliderForCrouch();
    }
    
    #endregion
}

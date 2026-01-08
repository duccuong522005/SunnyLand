using UnityEngine;

/// <summary>
/// Manages player animation states and sprite flipping.
/// Handles transitions between Idle, Run, Jump, Fall, Climb, Crouch, and Hurt states.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    #region Constants - Animation Parameter Names
    
    private const string ANIM_PARAM_SPEED = "Speed";
    private const string ANIM_PARAM_IS_GROUNDED = "IsGrounded";
    private const string ANIM_PARAM_IS_JUMPING = "IsJumping";
    private const string ANIM_PARAM_IS_FALLING = "IsFalling";
    private const string ANIM_PARAM_IS_CLIMBING = "IsClimbing";
    private const string ANIM_PARAM_IS_CROUCHING = "IsCrouching";
    private const string ANIM_PARAM_IS_HURT = "IsHurt";
    
    #endregion
    
    #region Inspector Settings
    
    [Header("References")]
    [Tooltip("Animator component for controlling animation states")]
    [SerializeField] private Animator _animator;
    
    [Tooltip("Player controller for movement input data")]
    [SerializeField] private PlayerController _playerController;
    
    [Tooltip("Ground check for grounded state detection")]
    [SerializeField] private GroundCheck _groundCheck;
    
    #endregion
    
    #region Private Fields
    
    private float _lastYPosition;
    private bool _isFalling;
    private bool _hasAnimatorController;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        CacheComponents();
        InitializePositionTracking();
    }
    
    private void Update()
    {
        UpdateAnimationParameters();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Cache all required components in Awake for performance
    /// </summary>
    private void CacheComponents()
    {
        if (_animator == null)
        {
            TryGetComponent(out _animator);
        }
        
        if (_playerController == null)
        {
            TryGetComponent(out _playerController);
        }
        
        if (_groundCheck == null)
        {
            TryGetComponent(out _groundCheck);
        }
        
        ValidateAnimator();
    }
    
    /// <summary>
    /// Validate animator and cache controller state
    /// </summary>
    private void ValidateAnimator()
    {
        if (_animator != null)
        {
            _hasAnimatorController = _animator.runtimeAnimatorController != null;
        }
    }
    
    /// <summary>
    /// Initialize position tracking for jump/fall detection
    /// </summary>
    private void InitializePositionTracking()
    {
        _lastYPosition = transform.position.y;
    }
    
    #endregion
    
    #region Animation Updates
    
    /// <summary>
    /// Update all animation parameters based on player state
    /// </summary>
    private void UpdateAnimationParameters()
    {
        if (!IsValidForAnimation()) return;
        
        UpdateSpeedParameter();
        UpdateGroundedParameter();
        UpdateJumpAndFallParameters();
    }
    
    /// <summary>
    /// Check if animation system is ready to receive updates
    /// </summary>
    private bool IsValidForAnimation()
    {
        return _animator != null && 
               _playerController != null && 
               _hasAnimatorController;
    }
    
    /// <summary>
    /// Update speed parameter based on horizontal movement
    /// </summary>
    private void UpdateSpeedParameter()
    {
        float speed = Mathf.Abs(_playerController.HorizontalInput);
        _animator.SetFloat(ANIM_PARAM_SPEED, speed);
    }
    
    /// <summary>
    /// Update grounded state parameter
    /// </summary>
    private void UpdateGroundedParameter()
    {
        bool isGrounded = _groundCheck != null && _groundCheck.IsGrounded();
        _animator.SetBool(ANIM_PARAM_IS_GROUNDED, isGrounded);
    }
    
    /// <summary>
    /// Update jump and fall parameters based on vertical velocity
    /// </summary>
    private void UpdateJumpAndFallParameters()
    {
        float currentY = transform.position.y;
        bool isGrounded = _groundCheck != null && _groundCheck.IsGrounded();
        
        if (!isGrounded)
        {
            UpdateAirborneStates(currentY);
        }
        else
        {
            ResetAirborneStates();
        }
        
        _lastYPosition = currentY;
    }
    
    /// <summary>
    /// Update animation states when player is in air
    /// </summary>
    private void UpdateAirborneStates(float currentY)
    {
        if (currentY < _lastYPosition)
        {
            // Falling
            _isFalling = true;
            SetAirborneParameters(isJumping: false, isFalling: true);
        }
        else if (currentY > _lastYPosition)
        {
            // Jumping up
            _isFalling = false;
            SetAirborneParameters(isJumping: true, isFalling: false);
        }
    }
    
    /// <summary>
    /// Reset airborne animation states when grounded
    /// </summary>
    private void ResetAirborneStates()
    {
        _isFalling = false;
        SetAirborneParameters(isJumping: false, isFalling: false);
    }
    
    /// <summary>
    /// Set jump and fall animation parameters
    /// </summary>
    private void SetAirborneParameters(bool isJumping, bool isFalling)
    {
        if (!_hasAnimatorController) return;
        
        _animator.SetBool(ANIM_PARAM_IS_JUMPING, isJumping);
        _animator.SetBool(ANIM_PARAM_IS_FALLING, isFalling);
    }
    
    #endregion
    
    #region Public Animation Controls
    
    /// <summary>
    /// Set climbing animation state
    /// </summary>
    /// <param name="isClimbing">True if player is climbing</param>
    public void SetClimbing(bool isClimbing)
    {
        if (!_hasAnimatorController) return;
        _animator.SetBool(ANIM_PARAM_IS_CLIMBING, isClimbing);
    }
    
    /// <summary>
    /// Set crouching animation state
    /// </summary>
    /// <param name="isCrouching">True if player is crouching</param>
    public void SetCrouching(bool isCrouching)
    {
        if (!_hasAnimatorController) return;
        _animator.SetBool(ANIM_PARAM_IS_CROUCHING, isCrouching);
    }
    
    /// <summary>
    /// Trigger hurt animation
    /// </summary>
    public void TriggerHurt()
    {
        if (!_hasAnimatorController) return;
        _animator.SetTrigger(ANIM_PARAM_IS_HURT);
    }
    
    /// <summary>
    /// Flip sprite horizontally based on movement direction
    /// </summary>
    /// <param name="horizontalInput">Horizontal input value (-1 to 1)</param>
    public void FlipSprite(float horizontalInput)
    {
        Vector3 currentScale = transform.localScale;
        
        if (horizontalInput > 0.01f)
        {
            // Moving right - normal scale
            currentScale.x = Mathf.Abs(currentScale.x);
        }
        else if (horizontalInput < -0.01f)
        {
            // Moving left - flip sprite
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        
        transform.localScale = currentScale;
    }
    
    #endregion
}

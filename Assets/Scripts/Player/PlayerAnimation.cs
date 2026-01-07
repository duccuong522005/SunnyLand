using UnityEngine;

/// <summary>
/// Script điều khiển animations của player
/// Quản lý các animation states: Idle, Run, Jump, Fall, Climb, Crouch, Hurt
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GroundCheck groundCheck;
    
    [Header("Animation Parameters")]
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_IS_GROUNDED = "IsGrounded";
    private const string ANIM_IS_JUMPING = "IsJumping";
    private const string ANIM_IS_FALLING = "IsFalling";
    private const string ANIM_IS_CLIMBING = "IsClimbing";
    private const string ANIM_IS_CROUCHING = "IsCrouching";
    private const string ANIM_IS_HURT = "IsHurt";
    
    private float lastYPosition;
    private bool isFalling;
    
    private void Awake()
    {
        // Tự động lấy references nếu chưa được gán
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        
        if (groundCheck == null)
        {
            groundCheck = GetComponent<GroundCheck>();
        }
        
        lastYPosition = transform.position.y;
    }
    
    private void Update()
    {
        UpdateAnimationParameters();
    }
    
    /// <summary>
    /// Cập nhật tất cả animation parameters dựa trên state của player
    /// </summary>
    private void UpdateAnimationParameters()
    {
        if (animator == null || playerController == null) return;
        
        // Kiểm tra xem Animator có Controller chưa để tránh warnings
        if (animator.runtimeAnimatorController == null) return;
        
        // Speed parameter - tốc độ di chuyển ngang
        float speed = Mathf.Abs(playerController.HorizontalInput);
        animator.SetFloat(ANIM_SPEED, speed);
        
        // Ground check
        bool isGrounded = groundCheck != null && groundCheck.IsGrounded();
        animator.SetBool(ANIM_IS_GROUNDED, isGrounded);
        
        // Jump và Fall detection
        float currentY = transform.position.y;
        bool wasFalling = isFalling;
        
        if (!isGrounded)
        {
            if (animator.runtimeAnimatorController != null)
            {
                if (currentY < lastYPosition)
                {
                    // Đang rơi
                    isFalling = true;
                    animator.SetBool(ANIM_IS_FALLING, true);
                    animator.SetBool(ANIM_IS_JUMPING, false);
                }
                else if (currentY > lastYPosition)
                {
                    // Đang nhảy lên
                    isFalling = false;
                    animator.SetBool(ANIM_IS_JUMPING, true);
                    animator.SetBool(ANIM_IS_FALLING, false);
                }
            }
        }
        else
        {
            // Trên mặt đất
            isFalling = false;
            if (animator.runtimeAnimatorController != null)
            {
                animator.SetBool(ANIM_IS_JUMPING, false);
                animator.SetBool(ANIM_IS_FALLING, false);
            }
        }
        
        lastYPosition = currentY;
    }
    
    /// <summary>
    /// Set animation state cho climbing
    /// </summary>
    public void SetClimbing(bool isClimbing)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(ANIM_IS_CLIMBING, isClimbing);
        }
    }
    
    /// <summary>
    /// Set animation state cho crouching
    /// </summary>
    public void SetCrouching(bool isCrouching)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(ANIM_IS_CROUCHING, isCrouching);
        }
    }
    
    /// <summary>
    /// Trigger hurt animation
    /// </summary>
    public void TriggerHurt()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger(ANIM_IS_HURT);
        }
    }
    
    /// <summary>
    /// Flip sprite dựa trên hướng di chuyển
    /// </summary>
    public void FlipSprite(float horizontalInput)
    {
        if (horizontalInput > 0)
        {
            // Di chuyển sang phải - giữ nguyên scale
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            // Di chuyển sang trái - flip sprite
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}


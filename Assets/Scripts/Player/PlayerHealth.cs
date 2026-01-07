using UnityEngine;
using System;

/// <summary>
/// Script quản lý máu và sát thương của player
/// Xử lý nhận sát thương, hồi máu, và sự kiện chết
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f; // Thời gian bất tử sau khi bị thương
    
    [Header("References")]
    [SerializeField] private PlayerAnimation playerAnimation;
    
    // Events
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnPlayerDied;
    public event Action OnPlayerHurt;
    
    // Private variables
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    
    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvincible => isInvincible;
    
    private void Awake()
    {
        // Khởi tạo máu đầy
        currentHealth = maxHealth;
        
        // Tự động tìm PlayerAnimation nếu chưa được gán
        if (playerAnimation == null)
        {
            playerAnimation = GetComponent<PlayerAnimation>();
        }
    }
    
    private void Update()
    {
        HandleInvincibility();
    }
    
    /// <summary>
    /// Xử lý thời gian bất tử sau khi bị thương
    /// </summary>
    private void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }
    
    /// <summary>
    /// Nhận sát thương
    /// </summary>
    /// <param name="damage">Lượng sát thương nhận vào</param>
    public void TakeDamage(int damage)
    {
        // Không nhận sát thương nếu đang bất tử hoặc đã chết
        if (isInvincible || IsDead) return;
        
        // Trừ máu
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Kích hoạt bất tử
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        
        // Trigger hurt animation
        if (playerAnimation != null)
        {
            playerAnimation.TriggerHurt();
        }
        
        // Gọi events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerHurt?.Invoke();
        
        // Kiểm tra nếu chết
        if (IsDead)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Hồi máu
    /// </summary>
    /// <param name="healAmount">Lượng máu hồi</param>
    public void Heal(int healAmount)
    {
        if (IsDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Hồi máu đầy
    /// </summary>
    public void HealFull()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Xử lý khi player chết
    /// </summary>
    private void Die()
    {
        OnPlayerDied?.Invoke();
        
        // Có thể thêm logic khác ở đây:
        // - Disable player controller
        // - Play death animation
        // - Show game over UI
        // - Reload scene sau một khoảng thời gian
    }
    
    /// <summary>
    /// Hồi sinh player
    /// </summary>
    public void Respawn()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        invincibilityTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Set máu tối đa (có thể dùng khi level up)
    /// </summary>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}


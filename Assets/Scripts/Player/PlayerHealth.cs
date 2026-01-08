using UnityEngine;
using System;

/// <summary>
/// Manages player health, damage, healing, and death states.
/// Handles invincibility frames and health change events.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    #region Constants
    
    private const int MIN_HEALTH = 0;
    
    #endregion
    
    #region Inspector Settings
    
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    [Range(1, 1000)]
    [SerializeField] private int _maxHealth = 100;
    
    [Tooltip("Duration of invincibility after taking damage (seconds)")]
    [Range(0f, 5f)]
    [SerializeField] private float _invincibilityDuration = 1f;
    
    [Header("References")]
    [Tooltip("Player animation component for hurt animation trigger")]
    [SerializeField] private PlayerAnimation _playerAnimation;
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Invoked when health changes. Parameters: (currentHealth, maxHealth)
    /// </summary>
    public event Action<int, int> OnHealthChanged;
    
    /// <summary>
    /// Invoked when player dies
    /// </summary>
    public event Action OnPlayerDied;
    
    /// <summary>
    /// Invoked when player takes damage (before death check)
    /// </summary>
    public event Action OnPlayerHurt;
    
    #endregion
    
    #region Private Fields
    
    private int _currentHealth;
    private bool _isInvincible;
    private float _invincibilityTimer;
    
    #endregion
    
    #region Public Properties
    
    /// <summary>
    /// Current health value
    /// </summary>
    public int CurrentHealth => _currentHealth;
    
    /// <summary>
    /// Maximum health value
    /// </summary>
    public int MaxHealth => _maxHealth;
    
    /// <summary>
    /// Returns true if player is dead (health <= 0)
    /// </summary>
    public bool IsDead => _currentHealth <= MIN_HEALTH;
    
    /// <summary>
    /// Returns true if player is currently invincible
    /// </summary>
    public bool IsInvincible => _isInvincible;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeHealth();
        CacheComponents();
    }
    
    private void Update()
    {
        UpdateInvincibility();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize health to maximum value
    /// </summary>
    private void InitializeHealth()
    {
        _currentHealth = _maxHealth;
    }
    
    /// <summary>
    /// Cache required components
    /// </summary>
    private void CacheComponents()
    {
        if (_playerAnimation == null)
        {
            TryGetComponent(out _playerAnimation);
        }
    }
    
    #endregion
    
    #region Health Management
    
    /// <summary>
    /// Apply damage to player. Respects invincibility and death states.
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(int damage)
    {
        if (!CanTakeDamage()) return;
        
        ApplyDamage(damage);
        ActivateInvincibility();
        TriggerHurtAnimation();
        NotifyHealthChanged();
        NotifyPlayerHurt();
        
        if (IsDead)
        {
            HandleDeath();
        }
    }
    
    /// <summary>
    /// Check if player can receive damage
    /// </summary>
    private bool CanTakeDamage()
    {
        return !_isInvincible && !IsDead && _currentHealth > MIN_HEALTH;
    }
    
    /// <summary>
    /// Apply damage amount to current health
    /// </summary>
    private void ApplyDamage(int damage)
    {
        _currentHealth = Mathf.Max(MIN_HEALTH, _currentHealth - damage);
    }
    
    /// <summary>
    /// Restore health points
    /// </summary>
    /// <param name="healAmount">Amount of health to restore</param>
    public void Heal(int healAmount)
    {
        if (IsDead || healAmount <= 0) return;
        
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + healAmount);
        NotifyHealthChanged();
    }
    
    /// <summary>
    /// Restore health to maximum
    /// </summary>
    public void HealFull()
    {
        if (IsDead) return;
        
        _currentHealth = _maxHealth;
        NotifyHealthChanged();
    }
    
    /// <summary>
    /// Set new maximum health value (e.g., for level up)
    /// </summary>
    /// <param name="newMaxHealth">New maximum health value</param>
    public void SetMaxHealth(int newMaxHealth)
    {
        if (newMaxHealth < 1)
        {
            if (Debug.isDebugBuild)
            {
                Debug.LogWarning($"[PlayerHealth] Invalid max health value: {newMaxHealth}. Must be >= 1.");
            }
            return;
        }
        
        _maxHealth = newMaxHealth;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
        NotifyHealthChanged();
    }
    
    #endregion
    
    #region Invincibility
    
    /// <summary>
    /// Update invincibility timer
    /// </summary>
    private void UpdateInvincibility()
    {
        if (!_isInvincible) return;
        
        _invincibilityTimer -= Time.deltaTime;
        if (_invincibilityTimer <= 0f)
        {
            _isInvincible = false;
        }
    }
    
    /// <summary>
    /// Activate invincibility frame period
    /// </summary>
    private void ActivateInvincibility()
    {
        _isInvincible = true;
        _invincibilityTimer = _invincibilityDuration;
    }
    
    #endregion
    
    #region Death & Respawn
    
    /// <summary>
    /// Handle player death logic
    /// </summary>
    private void HandleDeath()
    {
        OnPlayerDied?.Invoke();
        
        // Additional death logic can be added here:
        // - Disable player controller
        // - Play death animation
        // - Show game over UI
        // - Trigger respawn after delay
    }
    
    /// <summary>
    /// Respawn player with full health and reset invincibility
    /// </summary>
    public void Respawn()
    {
        _currentHealth = _maxHealth;
        _isInvincible = false;
        _invincibilityTimer = 0f;
        NotifyHealthChanged();
    }
    
    #endregion
    
    #region Animation
    
    /// <summary>
    /// Trigger hurt animation if animation component exists
    /// </summary>
    private void TriggerHurtAnimation()
    {
        _playerAnimation?.TriggerHurt();
    }
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Notify listeners of health change
    /// </summary>
    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    
    /// <summary>
    /// Notify listeners that player was hurt
    /// </summary>
    private void NotifyPlayerHurt()
    {
        OnPlayerHurt?.Invoke();
    }
    
    #endregion
}

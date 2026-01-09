using UnityEngine;
using SunnyLand.Utilities;

/// <summary>
/// Camera controller that smoothly follows a target with optional bounds and look-ahead.
/// Designed for 2D platformer games with responsive camera movement.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    #region Inspector Settings
    
    [Header("Target Settings")]
    [Tooltip("Transform to follow (usually the player)")]
    [SerializeField] private Transform _target;
    
    [Header("Follow Settings")]
    [Tooltip("Smoothness of camera follow (lower = smoother, higher = snappier)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float _smoothSpeed = 0.125f;
    
    [Tooltip("Offset from target position")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10);
    
    [Header("Bounds Settings")]
    [Tooltip("Enable camera bounds clamping")]
    [SerializeField] private bool _useBounds = false;
    
    [Tooltip("Minimum X position")]
    [SerializeField] private float _minX = -10f;
    
    [Tooltip("Maximum X position")]
    [SerializeField] private float _maxX = 10f;
    
    [Tooltip("Minimum Y position")]
    [SerializeField] private float _minY = -10f;
    
    [Tooltip("Maximum Y position")]
    [SerializeField] private float _maxY = 10f;
    
    [Header("Look Ahead Settings")]
    [Tooltip("Enable camera look-ahead when player moves")]
    [SerializeField] private bool _useLookAhead = true;
    
    [Tooltip("Distance to look ahead in movement direction")]
    [Range(0f, 10f)]
    [SerializeField] private float _lookAheadDistance = 2f;
    
    [Tooltip("Smoothness of look-ahead movement")]
    [Range(1f, 20f)]
    [SerializeField] private float _lookAheadSmooth = 5f;
    
    #endregion
    
    #region Private Fields
    
    private Vector3 _velocity = Vector3.zero;
    private float _currentLookAhead;
    private Camera _camera;
    private PlayerController _cachedPlayerController;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        CacheCamera();
        FindTargetIfNeeded();
        CachePlayerController();
    }
    
    private void LateUpdate()
    {
        if (_target == null) return;
        
        Vector3 desiredPosition = CalculateDesiredPosition();
        ApplyBounds(ref desiredPosition);
        ApplySmoothFollow(desiredPosition);
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Cache camera component for performance
    /// </summary>
    private void CacheCamera()
    {
        _camera = GetComponent<Camera>();
        
        if (_camera == null && Debug.isDebugBuild)
        {
            Debug.LogWarning($"[CameraFollow] Camera component not found on {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Automatically find player if target not assigned
    /// </summary>
    private void FindTargetIfNeeded()
    {
        if (_target != null) return;
        
        GameObject player = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
        if (player != null)
        {
            _target = player.transform;
            CachePlayerController();
        }
        else if (Debug.isDebugBuild)
        {
            Debug.LogWarning($"[CameraFollow] No target assigned and no GameObject with '{GameConstants.TAG_PLAYER}' tag found.");
        }
    }
    
    /// <summary>
    /// Cache player controller for look-ahead calculations
    /// </summary>
    private void CachePlayerController()
    {
        if (_target != null)
        {
            _target.TryGetComponent(out _cachedPlayerController);
        }
    }
    
    #endregion
    
    #region Position Calculation
    
    /// <summary>
    /// Calculate desired camera position with offset and look-ahead
    /// </summary>
    private Vector3 CalculateDesiredPosition()
    {
        Vector3 desiredPosition = _target.position + _offset;
        
        if (_useLookAhead)
        {
            ApplyLookAhead(ref desiredPosition);
        }
        
        return desiredPosition;
    }
    
    /// <summary>
    /// Apply look-ahead offset based on player movement direction
    /// </summary>
    private void ApplyLookAhead(ref Vector3 desiredPosition)
    {
        if (_cachedPlayerController == null) return;
        
        float targetLookAhead = _cachedPlayerController.HorizontalInput * _lookAheadDistance;
        _currentLookAhead = Mathf.Lerp(_currentLookAhead, targetLookAhead, _lookAheadSmooth * Time.deltaTime);
        desiredPosition.x += _currentLookAhead;
    }
    
    /// <summary>
    /// Clamp position within bounds if enabled
    /// </summary>
    private void ApplyBounds(ref Vector3 position)
    {
        if (!_useBounds) return;
        
        position.x = Mathf.Clamp(position.x, _minX, _maxX);
        position.y = Mathf.Clamp(position.y, _minY, _maxY);
    }
    
    /// <summary>
    /// Apply smooth following using SmoothDamp
    /// </summary>
    private void ApplySmoothFollow(Vector3 desiredPosition)
    {
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothSpeed);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Set new target for camera to follow
    /// </summary>
    /// <param name="newTarget">New target transform</param>
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
        CachePlayerController();
    }
    
    /// <summary>
    /// Set camera bounds and enable bounds clamping
    /// </summary>
    /// <param name="minX">Minimum X position</param>
    /// <param name="maxX">Maximum X position</param>
    /// <param name="minY">Minimum Y position</param>
    /// <param name="maxY">Maximum Y position</param>
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
        _useBounds = true;
    }
    
    #endregion
    
    #region Debug Visualization
    
    /// <summary>
    /// Draw bounds gizmo in Scene view when selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!_useBounds) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((_minX + _maxX) * 0.5f, (_minY + _maxY) * 0.5f, 0);
        Vector3 size = new Vector3(_maxX - _minX, _maxY - _minY, 0);
        Gizmos.DrawWireCube(center, size);
    }
    
    #endregion
}

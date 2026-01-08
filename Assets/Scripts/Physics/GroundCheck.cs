using UnityEngine;

/// <summary>
/// Utility component for detecting ground collisions using 2D physics overlap.
/// Provides ground detection for platformer character controllers.
/// </summary>
public class GroundCheck : MonoBehaviour
{
    #region Inspector Settings
    
    [Header("Ground Check Settings")]
    [Tooltip("Radius of the overlap circle for ground detection")]
    [Range(0.05f, 1f)]
    [SerializeField] private float _checkRadius = 0.2f;
    
    [Tooltip("Offset from transform position for check position")]
    [SerializeField] private Vector2 _checkOffset = Vector2.zero;
    
    [Tooltip("Layer mask containing all ground layers")]
    [SerializeField] private LayerMask _groundLayer = 1;
    
    [Header("Debug")]
    [Tooltip("Show ground check gizmo in Scene view")]
    [SerializeField] private bool _showGizmos = true;
    
    [Tooltip("Color of the ground check gizmo")]
    [SerializeField] private Color _gizmoColor = Color.green;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Check if GameObject is currently grounded
    /// </summary>
    /// <returns>True if overlapping with ground layer</returns>
    public bool IsGrounded()
    {
        Vector2 checkPosition = GetCheckPosition();
        Collider2D hit = Physics2D.OverlapCircle(checkPosition, _checkRadius, _groundLayer);
        return hit != null;
    }
    
    /// <summary>
    /// Get the Collider2D of the ground currently being touched (if any)
    /// </summary>
    /// <returns>Ground Collider2D or null if not grounded</returns>
    public Collider2D GetGroundCollider()
    {
        Vector2 checkPosition = GetCheckPosition();
        return Physics2D.OverlapCircle(checkPosition, _checkRadius, _groundLayer);
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Calculate the world position for ground check
    /// </summary>
    private Vector2 GetCheckPosition()
    {
        return (Vector2)transform.position + _checkOffset;
    }
    
    #endregion
    
    #region Debug Visualization
    
    /// <summary>
    /// Draw gizmo in Scene view for visual debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        
        Gizmos.color = _gizmoColor;
        Vector2 checkPosition = GetCheckPosition();
        Gizmos.DrawWireSphere(checkPosition, _checkRadius);
    }
    
    #endregion
}

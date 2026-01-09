using UnityEngine;
using UnityEngine.Serialization;
using SunnyLand.Utilities;

/// <summary>
/// Utility component for detecting ground collisions using 2D physics BoxCast.
/// Uses downward-only detection to prevent false positives when standing at platform edges.
/// Provides ground detection for platformer character controllers.
/// </summary>
public class GroundCheck : MonoBehaviour
{
    #region Constants
    
    private const float CAST_ANGLE = 0f;
    
    #endregion
    
    #region Inspector Settings
    
    [Header("Ground Check Settings")]
    [Tooltip("Width of the box cast for ground detection (horizontal size)")]
    [Range(0.05f, 1f)]
    [SerializeField]
    [FormerlySerializedAs("_checkRadius")]
    private float _checkWidth = 0.2f;
    
    [Tooltip("Offset from transform position for check position")]
    [SerializeField] private Vector2 _checkOffset = Vector2.zero;
    
    [Tooltip("Distance to check downward for ground")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float _checkDistance = 0.15f;
    
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
    /// Check if GameObject is currently grounded by casting downward.
    /// </summary>
    /// <returns>True if ground is detected below the check position</returns>
    public bool IsGrounded()
    {
        RaycastHit2D hit = PerformGroundCast();
        return hit.collider != null;
    }
    
    /// <summary>
    /// Get the Collider2D of the ground currently being touched (if any).
    /// </summary>
    /// <returns>Ground Collider2D or null if not grounded</returns>
    public Collider2D GetGroundCollider()
    {
        RaycastHit2D hit = PerformGroundCast();
        return hit.collider;
    }
    
    /// <summary>
    /// Get detailed information about the ground hit.
    /// </summary>
    /// <returns>RaycastHit2D with information about the ground collision, or empty if no ground detected</returns>
    public RaycastHit2D GetGroundHit()
    {
        return PerformGroundCast();
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Perform the actual ground detection using BoxCast downward.
    /// Centralizes the cast logic to avoid code duplication.
    /// </summary>
    /// <returns>RaycastHit2D result from the ground check</returns>
    private RaycastHit2D PerformGroundCast()
    {
        Vector2 checkPosition = GetCheckPosition();
        Vector2 boxSize = CalculateBoxSize();
        
        // Use BoxCast to only check downward, not from sides
        // This prevents false positives when standing at platform edges
        return Physics2D.BoxCast(
            origin: checkPosition,
            size: boxSize,
            angle: CAST_ANGLE,
            direction: Vector2.down,
            distance: _checkDistance,
            layerMask: _groundLayer
        );
    }
    
    /// <summary>
    /// Calculate the box size for the ground check cast.
    /// Box is narrower horizontally to be more precise at edges.
    /// </summary>
    /// <returns>Vector2 representing the width and height of the check box</returns>
    private Vector2 CalculateBoxSize()
    {
        return new Vector2(
            _checkWidth * GameConstants.BOX_WIDTH_MULTIPLIER,
            _checkWidth * GameConstants.BOX_HEIGHT_MULTIPLIER
        );
    }
    
    /// <summary>
    /// Calculate the world position for ground check based on transform and offset.
    /// </summary>
    /// <returns>World position where the ground check should be performed</returns>
    private Vector2 GetCheckPosition()
    {
        return (Vector2)transform.position + _checkOffset;
    }
    
    #endregion
    
    #region Debug Visualization
    
    /// <summary>
    /// Draw gizmo in Scene view for visual debugging of ground check area.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        
        Gizmos.color = _gizmoColor;
        Vector2 checkPosition = GetCheckPosition();
        Vector2 boxSize = CalculateBoxSize();
        Vector2 boxCenter = checkPosition + Vector2.down * (_checkDistance * 0.5f);
        
        // Draw the box cast area
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = originalMatrix;
        
        // Draw line showing check direction
        Gizmos.DrawLine(checkPosition, checkPosition + Vector2.down * _checkDistance);
    }
    
    #endregion
}
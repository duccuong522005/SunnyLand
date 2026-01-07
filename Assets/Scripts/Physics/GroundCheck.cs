using UnityEngine;

/// <summary>
/// Utility script để kiểm tra xem GameObject có đang đứng trên mặt đất không
/// Sử dụng Raycast hoặc OverlapCircle để phát hiện ground
/// </summary>
public class GroundCheck : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private Vector2 checkOffset = Vector2.zero;
    [SerializeField] private LayerMask groundLayer = 1; // Default layer
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.green;
    
    /// <summary>
    /// Kiểm tra xem có đang đứng trên mặt đất không
    /// </summary>
    public bool IsGrounded()
    {
        Vector2 checkPosition = (Vector2)transform.position + checkOffset;
        Collider2D hit = Physics2D.OverlapCircle(checkPosition, checkRadius, groundLayer);
        return hit != null;
    }
    
    /// <summary>
    /// Lấy thông tin Collider2D của mặt đất (nếu có)
    /// </summary>
    public Collider2D GetGroundCollider()
    {
        Vector2 checkPosition = (Vector2)transform.position + checkOffset;
        return Physics2D.OverlapCircle(checkPosition, checkRadius, groundLayer);
    }
    
    /// <summary>
    /// Vẽ gizmos trong Scene view để debug
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        Vector2 checkPosition = (Vector2)transform.position + checkOffset;
        Gizmos.DrawWireSphere(checkPosition, checkRadius);
    }
}


using UnityEngine;

/// <summary>
/// Script để camera tự động theo dõi player
/// Hỗ trợ smooth follow và giới hạn vùng camera
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // Player transform
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;
    
    [Header("Look Ahead Settings")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmooth = 5f;
    
    // Private variables
    private Vector3 velocity = Vector3.zero;
    private float currentLookAhead = 0f;
    private Camera cam;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Tự động tìm player nếu chưa được gán
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // Tính toán vị trí mong muốn
        Vector3 desiredPosition = target.position + offset;
        
        // Look ahead - camera nhìn về phía trước khi player di chuyển
        if (useLookAhead)
        {
            // Lấy hướng di chuyển của player (nếu có PlayerController)
            PlayerController playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                float targetLookAhead = playerController.HorizontalInput * lookAheadDistance;
                currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSmooth * Time.deltaTime);
                desiredPosition.x += currentLookAhead;
            }
        }
        
        // Áp dụng bounds nếu được bật
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smooth follow
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;
    }
    
    /// <summary>
    /// Set target mới cho camera
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Set bounds cho camera
    /// </summary>
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }
    
    /// <summary>
    /// Vẽ gizmos để hiển thị bounds trong Scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
        Gizmos.DrawWireCube(center, size);
    }
}


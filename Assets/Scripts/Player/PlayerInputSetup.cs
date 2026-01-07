using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Helper script để tự động setup Player Input component
/// Chạy một lần khi game start để đảm bảo Input System được setup đúng
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerInputSetup : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    
    private PlayerInput playerInput;
    private PlayerController playerController;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        // Tự động tìm hoặc tạo Player Input component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        // Setup Input Actions
        if (inputActions != null)
        {
            playerInput.actions = inputActions;
        }
        else
        {
            // Tự động tìm Input Actions file
            InputActionAsset foundActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (foundActions == null)
            {
                // Tìm trong Assets
                var allActions = UnityEngine.Resources.FindObjectsOfTypeAll<InputActionAsset>();
                foreach (var asset in allActions)
                {
                    if (asset.name == "InputSystem_Actions")
                    {
                        foundActions = asset;
                        break;
                    }
                }
            }
            
            if (foundActions != null)
            {
                playerInput.actions = foundActions;
            }
            else
            {
                Debug.LogWarning("PlayerInputSetup: Không tìm thấy InputSystem_Actions. Vui lòng gán thủ công trong Inspector.");
            }
        }
        
        // Setup default action map
        if (!string.IsNullOrEmpty(actionMapName))
        {
            playerInput.defaultActionMap = actionMapName;
        }
        
        // Setup behavior: Invoke Unity Events
        playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        
        // Kết nối events với PlayerController
        SetupInputEvents();
    }
    
    /// <summary>
    /// Kết nối Input System events với PlayerController methods
    /// </summary>
    private void SetupInputEvents()
    {
        if (playerInput == null || playerController == null) return;
        
        // Lấy Input Actions
        var moveAction = playerInput.actions?[actionMapName + "/Move"];
        var jumpAction = playerInput.actions?[actionMapName + "/Jump"];
        
        if (moveAction != null)
        {
            moveAction.performed += ctx => playerController.OnMove(ctx);
            moveAction.canceled += ctx => playerController.OnMove(ctx);
        }
        
        if (jumpAction != null)
        {
            jumpAction.performed += ctx => playerController.OnJump(ctx);
            jumpAction.canceled += ctx => playerController.OnJump(ctx);
        }
    }
    
    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
    }
    
    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }
    }
}


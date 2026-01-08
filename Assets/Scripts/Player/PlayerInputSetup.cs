using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Helper component that automatically configures Player Input component.
/// Ensures Input System is properly set up at runtime.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerInputSetup : MonoBehaviour
{
    #region Constants
    
    private const string DEFAULT_ACTION_MAP_NAME = "Player";
    private const string INPUT_ACTIONS_ASSET_NAME = "InputSystem_Actions";
    
    #endregion
    
    #region Inspector Settings
    
    [Header("Input Settings")]
    [Tooltip("Input Action Asset to use. If not assigned, will search automatically.")]
    [SerializeField] private InputActionAsset _inputActions;
    
    [Tooltip("Name of the action map to use (default: 'Player')")]
    [SerializeField] private string _actionMapName = DEFAULT_ACTION_MAP_NAME;
    
    #endregion
    
    #region Private Fields
    
    private PlayerInput _playerInput;
    private PlayerController _playerController;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        CacheComponents();
        SetupPlayerInput();
        ConnectInputEvents();
    }
    
    private void OnEnable()
    {
        ActivateInput();
    }
    
    private void OnDisable()
    {
        DeactivateInput();
    }
    
    private void OnDestroy()
    {
        DisconnectInputEvents();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Cache required components in Awake
    /// </summary>
    private void CacheComponents()
    {
        _playerController = GetComponent<PlayerController>();
        
        if (_playerController == null)
        {
            Debug.LogError($"[PlayerInputSetup] PlayerController component not found on {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Setup Player Input component and assign Input Actions
    /// </summary>
    private void SetupPlayerInput()
    {
        GetOrCreatePlayerInput();
        AssignInputActions();
        ConfigurePlayerInput();
    }
    
    /// <summary>
    /// Get existing Player Input or create new one
    /// </summary>
    private void GetOrCreatePlayerInput()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            _playerInput = gameObject.AddComponent<PlayerInput>();
        }
    }
    
    /// <summary>
    /// Assign Input Action Asset to Player Input
    /// </summary>
    private void AssignInputActions()
    {
        if (_inputActions != null)
        {
            _playerInput.actions = _inputActions;
            return;
        }
        
        InputActionAsset foundActions = FindInputActionsAsset();
        if (foundActions != null)
        {
            _playerInput.actions = foundActions;
        }
        else
        {
            LogInputActionsNotFound();
        }
    }
    
    /// <summary>
    /// Search for Input Action Asset in project
    /// </summary>
    private InputActionAsset FindInputActionsAsset()
    {
        // Try Resources folder first
        InputActionAsset foundActions = Resources.Load<InputActionAsset>(INPUT_ACTIONS_ASSET_NAME);
        if (foundActions != null) return foundActions;
        
        // Search all loaded assets
        var allActions = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        foreach (var asset in allActions)
        {
            if (asset.name == INPUT_ACTIONS_ASSET_NAME)
            {
                return asset;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Log warning when Input Actions not found
    /// </summary>
    private void LogInputActionsNotFound()
    {
        if (Debug.isDebugBuild)
        {
            Debug.LogWarning($"[PlayerInputSetup] Could not find '{INPUT_ACTIONS_ASSET_NAME}'. Please assign manually in Inspector.");
        }
    }
    
    /// <summary>
    /// Configure Player Input component settings
    /// </summary>
    private void ConfigurePlayerInput()
    {
        if (_playerInput.actions == null) return;
        
        if (!string.IsNullOrEmpty(_actionMapName))
        {
            _playerInput.defaultActionMap = _actionMapName;
        }
        
        _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
    }
    
    #endregion
    
    #region Input Event Connection
    
    /// <summary>
    /// Connect Input System events to PlayerController methods
    /// </summary>
    private void ConnectInputEvents()
    {
        if (_playerInput == null || _playerController == null || _playerInput.actions == null) return;
        
        string moveActionPath = $"{_actionMapName}/Move";
        string jumpActionPath = $"{_actionMapName}/Jump";
        
        _moveAction = _playerInput.actions[moveActionPath];
        _jumpAction = _playerInput.actions[jumpActionPath];
        
        if (_moveAction != null)
        {
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
        }
        
        if (_jumpAction != null)
        {
            _jumpAction.performed += OnJumpPerformed;
            _jumpAction.canceled += OnJumpCanceled;
        }
    }
    
    /// <summary>
    /// Disconnect Input System events to prevent memory leaks
    /// </summary>
    private void DisconnectInputEvents()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMoveCanceled;
        }
        
        if (_jumpAction != null)
        {
            _jumpAction.performed -= OnJumpPerformed;
            _jumpAction.canceled -= OnJumpCanceled;
        }
    }
    
    #endregion
    
    #region Input Callbacks
    
    /// <summary>
    /// Handle move action performed
    /// </summary>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _playerController?.OnMove(context);
    }
    
    /// <summary>
    /// Handle move action canceled
    /// </summary>
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _playerController?.OnMove(context);
    }
    
    /// <summary>
    /// Handle jump action performed
    /// </summary>
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        _playerController?.OnJump(context);
    }
    
    /// <summary>
    /// Handle jump action canceled
    /// </summary>
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        _playerController?.OnJump(context);
    }
    
    #endregion
    
    #region Input Activation
    
    /// <summary>
    /// Activate input system
    /// </summary>
    private void ActivateInput()
    {
        _playerInput?.ActivateInput();
    }
    
    /// <summary>
    /// Deactivate input system
    /// </summary>
    private void DeactivateInput()
    {
        _playerInput?.DeactivateInput();
    }
    
    #endregion
}

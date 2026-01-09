using UnityEngine;

namespace SunnyLand.Utilities
{
    /// <summary>
    /// Centralized constants used throughout the game.
    /// Prevents magic numbers and strings scattered across codebase.
    /// </summary>
    public static class GameConstants
    {
        #region Animation Parameters
        
        public const string ANIM_PARAM_SPEED = "Speed";
        public const string ANIM_PARAM_IS_GROUNDED = "IsGrounded";
        public const string ANIM_PARAM_IS_JUMPING = "IsJumping";
        public const string ANIM_PARAM_IS_FALLING = "IsFalling";
        public const string ANIM_PARAM_IS_CLIMBING = "IsClimbing";
        public const string ANIM_PARAM_IS_CROUCHING = "IsCrouching";
        public const string ANIM_PARAM_IS_HURT = "IsHurt";
        
        #endregion
        
        #region Physics Constants
        
        public const float MIN_VELOCITY_Y_THRESHOLD = 0.01f;
        public const float VARIABLE_JUMP_MULTIPLIER = 0.5f;
        public const float MIN_INPUT_THRESHOLD = 0.01f;
        
        #endregion
        
        #region Ground Check Constants
        
        public const float BOX_WIDTH_MULTIPLIER = 0.8f;
        public const float BOX_HEIGHT_MULTIPLIER = 0.5f;
        
        #endregion
        
        #region Tags & Layers
        
        public const string TAG_PLAYER = "Player";
        
        #endregion
        
        #region Input Constants
        
        public const string DEFAULT_ACTION_MAP_NAME = "Player";
        public const string INPUT_ACTIONS_ASSET_NAME = "InputSystem_Actions";
        
        #endregion
    }
}


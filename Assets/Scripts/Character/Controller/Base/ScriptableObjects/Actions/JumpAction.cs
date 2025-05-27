///스크립트 생성 일자 - 2025 - 03 - 20
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using CHG.Utilities.Attribute;
using UnityEngine;

namespace TempNamespace.Character.Controller
{
	[CreateAssetMenu(fileName = "JumpAction", menuName = "Character/Action/Jump", order = 100)]
	public class JumpAction : CharacterAction
	{
		#region Inspector Fields
		
        [Header("Jump")]
        [SerializeField, Tooltip("공중에 뜬 상태에서도 점프가 가능한가?")]
        private bool _canJumpInAir;

        [SerializeField, Tooltip("최대 점프 회수")]
        private int _jumpMaxCount;
        [SerializeField, Tooltip("점프할 때 초기 수직 가속도")]
        private float _jumpImpulse;        
        [SerializeField, Tooltip("최대 점프 홀딩(점프 키를 눌러 수직 가속도 증가) 시간")]
        private float _jumpMaxHoldTime;        
        [SerializeField, Tooltip("점프 키의 선입력 허용치")]
        private float _jumpBuffering;
        [SerializeField, Tooltip("공중에 뜬 상황에서 점프 허용 시간(Coyote Time)"), ConditionalHide("NeedCoyoteTime")]
        private float _jumpCoyoteTime;
		#endregion

		#region Fields
		protected bool _isJumping;
        private float _jumpInputHoldTime;
        private float _jumpForceTimeRemaining;
        private int _jumpCurrentCount;

        protected float _fallingTime;
		#endregion
		
		#region Properties
		
        public bool NeedCoyoteTime
        {
            get => !_canJumpInAir;
        }
        /// <summary>
        /// 공중에 뜬 상태에서도 점프가 가능한가?
        /// </summary>
        public bool CanJumpInAir
        {
            get => _canJumpInAir;
            set => _canJumpInAir = value;
        }

        /// <summary>
        /// The max number of jumps the Character can perform.
        /// </summary>

        public int jumpMaxCount
        {
            get => _jumpMaxCount;
            set => _jumpMaxCount = Mathf.Max(1, value);
        }
        
        /// <summary>
        /// Initial velocity (instantaneous vertical velocity) when jumping.
        /// </summary>

        public float jumpImpulse
        {
            get => _jumpImpulse;
            set => _jumpImpulse = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// The maximum time (in seconds) to hold the jump. eg: Variable height jump.
        /// </summary>

        public float jumpMaxHoldTime
        {
            get => _jumpMaxHoldTime;
            set => _jumpMaxHoldTime = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// How early before hitting the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpMaxPreGroundedTime
        {
            get => _jumpBuffering;
            set => _jumpBuffering = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// How long after leaving the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpMaxPostGroundedTime
        {
            get => _jumpCoyoteTime;
            set => _jumpCoyoteTime = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// This is the time (in seconds) that the player has held the jump input.
        /// </summary>
        public float jumpInputHoldTime
        {
            get => _jumpInputHoldTime;
            protected set => _jumpInputHoldTime = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Amount of jump force time remaining, if jumpMaxHoldTime > 0.
        /// </summary>

        public float jumpForceTimeRemaining
        {
            get => _jumpForceTimeRemaining;
            protected set => _jumpForceTimeRemaining = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Tracks the current number of jumps performed.
        /// </summary>

        public int jumpCurrentCount
        {
            get => _jumpCurrentCount;
            protected set => _jumpCurrentCount = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Should notify a jump apex ?
        /// Set to true to receive OnReachedJumpApex event.
        /// </summary>

        public bool notifyJumpApex { get; set; }
        
        /// <summary>
        /// Is the jump input pressed?
        /// </summary>

        public bool jumpInputPressed { get; protected set; }

        /// <summary>
        /// The Character's time in falling movement mode.
        /// </summary>

        public float FallingTime => _fallingTime;

		#endregion

		#region Methods
		
        public override void OnBeforeSimulate(PhysicsBasedController moveSystem)
        {
			if(moveSystem.IsOnGround())
			{
				_fallingTime = 0;
			}
			else
			{
				_fallingTime += Time.fixedDeltaTime;
			}

            bool jumpKey = moveSystem.CheckBoolKey("Jump", false);
			if(jumpKey)
			{
				Jump();
			}
			else if(!jumpKey)
			{
				StopJumping(moveSystem);
			}

            CheckJumpInput(moveSystem);
            UpdateJumpTimers(moveSystem, Time.fixedDeltaTime);
        }

        public override void OnSimulate(PhysicsBasedController moveSystem)
        {
        }

        public override void OnAfterSimulate(PhysicsBasedController moveSystem)
        {
        }
		
        /// <summary>
        /// Check jump input and attempts to perform the requested jump.
        /// </summary>
        
        protected virtual void CheckJumpInput(PhysicsBasedController moveSystem)
        {
            if (!jumpInputPressed)
                return;
            
			//
			//공중 점프가 불가능한 상태에서 + 낙하시간이 Coyote Time을 지났고 + 2단 점프가 아니라면 Return
            if (!_canJumpInAir && jumpCurrentCount == 0 && !moveSystem.UseGrounding && FallingTime > jumpMaxPostGroundedTime)
                return;

            bool didJump = CanJump(moveSystem) && DoJump(moveSystem);
            if (didJump)
            {
                // Transition from not (actively) jumping to jumping

                if (!_isJumping)
                {
                    jumpCurrentCount++;
                    jumpForceTimeRemaining = jumpMaxHoldTime;
                    
                    moveSystem.physicsSystem.PauseGroundConstraint();
                }
            }

            _isJumping = didJump;
        }
        
        /// <summary>
        /// Update jump related timers
        /// </summary>
        
        protected virtual void UpdateJumpTimers(PhysicsBasedController moveSystem, float deltaTime)
        {
            if (jumpInputPressed)
                jumpInputHoldTime += deltaTime;
            
            if (jumpForceTimeRemaining > 0.0f)
            {
                jumpForceTimeRemaining -= deltaTime;
                if (jumpForceTimeRemaining <= 0.0f)
                    ResetJumpState(moveSystem);
            }
        }
        /// <summary>
        /// Reset jump related vars.
        /// </summary>
        
        protected virtual void ResetJumpState(PhysicsBasedController moveSystem)
        {
            if (moveSystem.IsOnWalkableGround())
                jumpCurrentCount = 0;

            jumpForceTimeRemaining = 0.0f;
            
            _isJumping = false;
        }

		
        /// <summary>
        /// Request the Character to jump. The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>
        public virtual void Jump()
        {
            jumpInputPressed = true;
        }
        
        /// <summary>
        /// Request the Character to end a jump. The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>
        public virtual void StopJumping(PhysicsBasedController moveSystem)
        {
            jumpInputPressed = false;
            jumpInputHoldTime = 0.0f;
            
            ResetJumpState(moveSystem);
        }
        
        
        /// <summary>
        /// True if jump is actively providing a force, such as when the jump input is held
        /// and the time it has been held is less than jumpMaxHoldTime.
        /// </summary>
        
        public virtual bool IsJumpProvidingForce()
        {
            return jumpForceTimeRemaining > 0.0f;
        }
        
        /// <summary>
        /// Compute the max jump height based on the jumpImpulse velocity and gravity.
        /// This does not take into account the jumpMaxHoldTime. 
        /// </summary>
        
        public virtual float GetMaxJumpHeight(PhysicsBasedController moveSystem)
        {
            float gravityMagnitude = moveSystem.GetGravityMagnitude();
            if (gravityMagnitude > 0.0001f)
            {
                return jumpImpulse * jumpImpulse / (2.0f * gravityMagnitude);
            }
            
            return 0.0f;
        }
        
        /// <summary>
        /// Compute the max jump height based on the jumpImpulse velocity and gravity.
        /// This does take into account the jumpMaxHoldTime. 
        /// </summary>
        
        public virtual float GetMaxJumpHeightWithJumpTime(PhysicsBasedController moveSystem)
        {
            float maxJumpHeight = GetMaxJumpHeight(moveSystem);
            return maxJumpHeight + jumpImpulse * jumpMaxHoldTime;
        }
        
        /// <summary>
        /// Determines if the Character is able to jump in its current state.
        /// </summary>
        
        protected virtual bool IsJumpAllowed(PhysicsBasedController moveSystem)
        {
            return moveSystem.UseGravity || moveSystem.IsOnWalkableGround();
        }
        
        /// <summary>
        /// Determines if the Character is able to perform the requested jump.
        /// </summary>
        
        protected virtual bool CanJump(PhysicsBasedController moveSystem)
        {
            // Ensure that the Character state is valid

            bool isJumpAllowed = IsJumpAllowed(moveSystem);
            if (isJumpAllowed)
            {
                // Ensure jumpCurrentCount and jumpInputHoldTime are valid
                
                if (!_isJumping || jumpMaxHoldTime <= 0.0f)
                {
                    if (jumpCurrentCount == 0)
                    {
                        // On first jump, jumpInputHoldTime MUST be within jumpMaxPreGroundedTime grace period
                        
                        isJumpAllowed = jumpInputHoldTime <= jumpMaxPreGroundedTime;
                        
                        // If is a valid jump, reset jumpInputHoldTime,
                        // otherwise jump hold will be inaccurate due jumpInputHoldTime not starting at zero
                        
                        if (isJumpAllowed)
                            jumpInputHoldTime = 0.0f;
                    }
                    else
                    {
                        // Consecutive jump, must be enough jumps and a new press (ie: jumpInputHoldTime == 0.0f)
                        
                        isJumpAllowed = jumpCurrentCount < jumpMaxCount && jumpInputHoldTime == 0.0f;
                    }
                }
                else
                {
                    // Only consider JumpInputHoldTime as long as:
                    // A) The jump limit hasn't been met OR
                    // B) The jump limit has been met AND we were already jumping

                    bool jumpInputHeld = jumpInputPressed && jumpInputHoldTime < jumpMaxHoldTime;
                
                    isJumpAllowed = jumpInputHeld && (jumpCurrentCount < jumpMaxCount || (_isJumping && jumpCurrentCount == jumpMaxCount));
                }
            }

            return isJumpAllowed;
        }
        
        /// <summary>
        /// Perform the jump applying jumpImpulse.
        /// This can be called multiple times in case jump is providing force (ie: variable height jump).
        /// </summary>
        
        protected virtual bool DoJump(PhysicsBasedController moveSystem)
        {
            // World up, determined by gravity direction
            
            Vector3 worldUp = -moveSystem.GetGravityDirection();
            
            // Don't jump if we can't move up/down.
            
            if (moveSystem.physicsSystem.IsConstrainedToPlane && 
                Mathf.Approximately(Vector3.Dot(moveSystem.physicsSystem.GetPlaneConstraintNormal(), worldUp), 1.0f))
            {
                return false;
            }
            
            // Apply jump impulse along world up defined by gravity direction
            
            float verticalSpeed = Mathf.Max(Vector3.Dot(moveSystem.physicsSystem.Velocity, worldUp), jumpImpulse);

            moveSystem.physicsSystem.Velocity =
                Vector3.ProjectOnPlane(moveSystem.physicsSystem.Velocity, worldUp) + worldUp * verticalSpeed;
            
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            _jumpMaxCount = 1;
            _jumpImpulse = 5.0f;
            _jumpMaxHoldTime = 0.0f;
            _jumpBuffering = 0.0f;
            _jumpCoyoteTime = 0.0f;
        }
        #endregion

        #region UnityEditor Only Methods
		#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate();
			
            jumpMaxCount = _jumpMaxCount;
            jumpImpulse = _jumpImpulse;
            jumpMaxHoldTime = _jumpMaxHoldTime;
            jumpMaxPreGroundedTime = _jumpBuffering;
            jumpMaxPostGroundedTime = _jumpCoyoteTime;
        }

		#endif
        #endregion
    }
}
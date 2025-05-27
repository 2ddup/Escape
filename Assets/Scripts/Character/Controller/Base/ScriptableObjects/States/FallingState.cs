///스크립트 생성 일자 - 2025 - 03 - 18
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;

namespace TempNamespace.Character.Controller
{
	[CreateAssetMenu(fileName = "Falling", menuName = "Character/MoveState/Falling", order = 100)]
	public class FallingState : LocomotionState
	{
		#region Inspector Fields
		[SerializeField, Tooltip("최대 낙하 속도")]
		private float _maxFallSpeed;
		
		/// <summary>
		/// 최대 낙하 속도
		/// </summary>
		public float MaxFallSpeed
		{
		   get => _maxFallSpeed;
		   set => _maxFallSpeed = value;
		}

		#endregion

		#region Fields
		#endregion
		
		#region Properties
		#endregion

		#region Methods
        public override void OnBeforeSimulate(PhysicsBasedController character)
        {
        }

        public override void OnSimulate(PhysicsBasedController character)
        {			
            // Current target velocity
            Vector3 desiredVelocity = character.GetDesiredVelocity();
            
            // World-up defined by gravity direction
            Vector3 worldUp = -character.GetGravityDirection();
            
            // On not walkable ground...
            if (character.IsOnGround() && !character.IsOnWalkableGround())
            {
                // If moving into the 'wall', limit contribution
                Vector3 groundNormal = character.physicsSystem.GroundNormal;

                if (Vector3.Dot(desiredVelocity, groundNormal) < 0.0f)
                {
                    // Allow movement parallel to the wall, but not into it because that may push us up
                    Vector3 groundNormal2D = Vector3.ProjectOnPlane(groundNormal, worldUp).normalized;
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, groundNormal2D);
                }

                // Make velocity calculations planar by projecting the up vector into non-walkable surface
                worldUp = Vector3.ProjectOnPlane(worldUp, groundNormal).normalized;
            }

            // Separate velocity into its components
            Vector3 verticalVelocity = Vector3.Project(character.physicsSystem.Velocity, worldUp);
            Vector3 lateralVelocity = character.physicsSystem.Velocity - verticalVelocity;

            // Update lateral velocity
            lateralVelocity = character.CalcVelocity(lateralVelocity, desiredVelocity, Friction, false, Time.deltaTime);

            // Update vertical velocity
            verticalVelocity += character.gravity * Time.deltaTime;

            // Don't exceed terminal velocity.
            float actualFallSpeed = MaxFallSpeed;
            if (character.physicsVolume)
                actualFallSpeed = character.physicsVolume.MaxFallSpeed;

            if (Vector3.Dot(verticalVelocity, worldUp) < -actualFallSpeed)
                verticalVelocity = Vector3.ClampMagnitude(verticalVelocity, actualFallSpeed);

            // Apply new velocity
            character.physicsSystem.Velocity = lateralVelocity + verticalVelocity;
        }

        public override void OnAfterSimulate(PhysicsBasedController character)
        {
        }

        public override void OnApplyMovement(PhysicsBasedController character)
        {
        }

		/// <summary>
		/// 중력을 사용하는 상황에서 + 땅을 밟은 게 아니라면 낙하 상태 돌입
		/// </summary>
        public override bool CanTransitionHere(PhysicsBasedController moveSystem)
        {
			if(!moveSystem.IsGrounded() && moveSystem.UseGravity)
			{
				return true;
			}
			return false;
        }

		/// <summary>
		/// 착지하면 종료
		/// </summary>
        public override bool IsFinished(PhysicsBasedController moveSystem)
        {
            if(moveSystem.IsGrounded())
            {
                return true;
            }
			return false;
        }
		#endregion

		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		public override void Reset()
		{
            base.Reset();
			
			StateFlag = MoveStateFlag.Locomotive;
            //_maxFallSpeed = 40.0f;
			MaxSpeed = 2.5f;
			MaxFallSpeed = 40;
			MaxDeceleration = 0;
			Friction = 0.3f;
		}
		public override void OnValidate()
		{
            base.OnValidate();
		}
        #endif
        #endregion
    }
}
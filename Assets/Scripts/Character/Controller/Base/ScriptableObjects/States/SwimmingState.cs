///스크립트 생성 일자 - 2025 - 03 - 18
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;

namespace TempNamespace.Character.Controller
{
	[CreateAssetMenu(fileName = "Swimming", menuName = "Character/MoveState/Swimming", order = 100)]
	public class SwimmingState : LocomotionState
	{
        #region Inspector Fields
        #endregion

        #region Fields
        bool _canSwim = false;
        #endregion

        #region Properties
        #endregion

        #region Events
        void OnPhysicVolumeChanged(PhysicsVolume volume)
        {
            if(volume != null && volume.IsFluid)
            {
                _canSwim = true;
            }
            else
            {
                _canSwim = false;
            }
        }
        #endregion
        
        #region Methods
        public override void OnActivated(PhysicsBasedController moveSystem)
        {
            base.OnActivated(moveSystem);
            moveSystem.PhysicsVolumeChanged += OnPhysicVolumeChanged;
        }
        public override void OnDeactivated(PhysicsBasedController moveSystem)
        {
            base.OnDeactivated(moveSystem);
            moveSystem.PhysicsVolumeChanged -= OnPhysicVolumeChanged;
        }
        public override void OnStateEnter(PhysicsBasedController moveSystem, MoveStateFlag prevStateFlag)
        {
            moveSystem.EnableGroundConstraint(false);
        }
        public override void OnBeforeSimulate(PhysicsBasedController character)
        {
        }

        public override void OnSimulate(PhysicsBasedController character)
        {			
            // Compute actual buoyancy factoring current immersion depth
            float depth = character.CalcImmersionDepth();
            float actualBuoyancy = character.buoyancy * character.physicsVolume.Density * depth;
            
            // Calculate new velocity

            Vector3 desiredVelocity = character.GetDesiredVelocity();
            Vector3 newVelocity = character.physicsSystem.Velocity;
            
            Vector3 worldUp = -character.GetGravityDirection();
            float verticalSpeed = Vector3.Dot(newVelocity, worldUp);
            
            if (verticalSpeed > MaxSpeed * 0.33f && actualBuoyancy > 0.0f)
            {
                // Damp positive vertical speed (out of water)

                verticalSpeed = Mathf.Max(MaxSpeed * 0.33f, verticalSpeed * depth * depth);
                newVelocity = Vector3.ProjectOnPlane(newVelocity, worldUp) + worldUp * verticalSpeed;
            }
            else if (depth < 0.65f)
            {
                // Damp positive vertical desired speed

                float verticalDesiredSpeed = Vector3.Dot(desiredVelocity, worldUp);
                
                desiredVelocity =
                    Vector3.ProjectOnPlane(desiredVelocity, worldUp) + worldUp * Mathf.Min(0.1f, verticalDesiredSpeed);
            }
            
            // Using root motion...

            if (character.useRootMotion && character.rootMotionController)
            {
                // Preserve current vertical velocity as we want to keep the effect of gravity

                Vector3 verticalVelocity = Vector3.Project(newVelocity, worldUp);

                // Updates new velocity

                newVelocity = Vector3.ProjectOnPlane(desiredVelocity, worldUp) + verticalVelocity;
            }
            else
            {
                // Actual friction

                float actualFriction = character.IsInWaterPhysicsVolume()
                    ? character.physicsVolume.Friction * depth
                    : Friction * depth;

                newVelocity = character.CalcVelocity(newVelocity, desiredVelocity, actualFriction, true, Time.fixedDeltaTime);
            }

            // If swimming freely, apply gravity acceleration scaled by (1.0f - actualBuoyancy)

            newVelocity += character.gravity * ((1.0f - actualBuoyancy) * Time.fixedDeltaTime);

            // Update velocity

            character.physicsSystem.Velocity = newVelocity;
        }

        public override void OnAfterSimulate(PhysicsBasedController character)
        {
        }

        public override void OnApplyMovement(PhysicsBasedController character)
        {
        }


		/// <summary>
		/// 수영 가능 상태라면 진입
		/// </summary>
        public override bool CanTransitionHere(PhysicsBasedController moveSystem)
        {
			if(_canSwim)
			{
				return true;
			}
			return false;
        }

		/// <summary>
		/// 수영 불가능한 상황이라면 탈출
		/// </summary>
        public override bool IsFinished(PhysicsBasedController moveSystem)
        {
            if(!_canSwim)
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
			
			StateFlag = MoveStateFlag.Locomotive | MoveStateFlag.DontUseGravity;
            
            MaxSpeed = 3.0f;
            MaxDeceleration = 0.0f;
            Friction = 0.0f;
		}
		public override void OnValidate()
		{
            base.OnValidate();
		}
        #endif
        #endregion
    }
}
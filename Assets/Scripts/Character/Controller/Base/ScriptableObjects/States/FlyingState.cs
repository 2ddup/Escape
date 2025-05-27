///스크립트 생성 일자 - 2025 - 03 - 18
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;

namespace TempNamespace.Character.Controller
{
    /// <summary>
    /// 중력을 무시하고 비행하는 상황
    /// </summary>
	[CreateAssetMenu(fileName = "Flying", menuName = "Character/MoveState/Flying", order = 100)]
	public class FlyingState : LocomotionState
	{
        #region Inspector Fields
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Methods

        public override void OnStateEnter(PhysicsBasedController moveSystem, MoveStateFlag prevStateFlag)
        {
            moveSystem.EnableGroundConstraint(false);
        }
        public override void OnStateExit(PhysicsBasedController moveSystem, MoveStateFlag nextStateFlag)
        {
            if(!nextStateFlag.HasFlag(MoveStateFlag.DontUseGravity))
            {
                moveSystem.EnableGroundConstraint(true);
            }            
        }
        public override void OnBeforeSimulate(PhysicsBasedController character)
        {
            Vector3 movementDirection = character.GetMovementDirection();
            // Vertical movement
            if(character.CheckBoolKey("Jump"))
            {                
                movementDirection += Vector3.up;
            }
            else if(character.CheckBoolKey("Crouch"))
            {
                movementDirection += Vector3.down;
            }
            
            character.SetMovementDirection(movementDirection);
        }

        public override void OnSimulate(PhysicsBasedController character)
        {			
            
            if (character.useRootMotion && character.rootMotionController)
                character.physicsSystem.Velocity = character.GetDesiredVelocity();
            else
            {
                float actualFriction = character.IsInWaterPhysicsVolume() ? character.physicsVolume.Friction : Friction;

                character.physicsSystem.Velocity 
                    = character.CalcVelocity(character.physicsSystem.Velocity, character.GetDesiredVelocity(), actualFriction, true, Time.fixedDeltaTime);
            }
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
			if(!moveSystem.IsGrounded() && moveSystem.UseGravity && moveSystem.CheckBoolKey("Jump"))
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
			
			StateFlag = MoveStateFlag.Locomotive | MoveStateFlag.DontUseGravity;

            MaxSpeed = 10.0f;
			MaxDeceleration = 0;
			Friction = 1.0f;
		}
		public override void OnValidate()
		{
            base.OnValidate();
		}
        #endif
        #endregion
    }
}
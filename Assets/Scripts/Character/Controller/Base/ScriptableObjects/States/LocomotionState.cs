///스크립트 생성 일자 - 2025 - 03 - 18
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using TempNamespace.Utilities;
using UnityEngine;

namespace TempNamespace.Character.Controller
{
    /// <summary>
    /// 기본적인 이동 모드, 땅 위를 걸어다니는 상황.
    /// Simulate 시점에 목표 Velocity에 맞춰 이동.
    /// </summary>
	[CreateAssetMenu(fileName = "Locomotion", menuName = "Character/MoveState/Locomotion", order = 100)]
	public class LocomotionState : MoveState
	{
		#region Inspector Fields
        [SerializeField, Tooltip("마찰에 따른 감속도")]
        private float _friction;
        
        /// <summary>
        /// 마찰에 따른 감속도
        /// </summary>
        public float Friction
        {
           get => _friction;
           set => _friction = value;
        }
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void OnStateEnter(PhysicsBasedController moveSystem, MoveStateFlag prevStateFlag)
        {
            ///중력을 적용하지 않는 상태였다면(≒지면 접지를 쓰고 있지 않았다면) 지면 접지를 재활성화
            if(prevStateFlag.HasFlag(MoveStateFlag.DontUseGravity))
            {
                moveSystem.EnableGroundConstraint(true);
            }
        }

        public override void OnBeforeSimulate(PhysicsBasedController moveSystem)
        {            
        }

        public override void OnSimulate(PhysicsBasedController moveSystem)
        {
            // RootMotion을 사용한다면 애니메이터가 주는 값에 맞춰 이동
            if (moveSystem.useRootMotion && moveSystem.rootMotionController)
                moveSystem.physicsSystem.Velocity = moveSystem.GetDesiredVelocity();

            else
            {
                //새 Velocity 계산
                moveSystem.physicsSystem.Velocity =
                    moveSystem.CalcVelocity(moveSystem.physicsSystem.Velocity, moveSystem.GetDesiredVelocity(), 
                    Friction, false, Time.deltaTime);
            }
            
            //아래로 내려가는 힘 적용
            if (moveSystem.applyStandingDownwardForce)
                moveSystem.ApplyDownwardsForce();
        }

        public override void OnAfterSimulate(PhysicsBasedController moveSystem)
        {

        }

        public override void OnApplyMovement(PhysicsBasedController moveSystem)
        {
            
        }
        /// <summary>
        /// 기본적으로 땅 
        /// </summary>
        public override bool CanTransitionHere(PhysicsBasedController moveSystem)
        {
			if(moveSystem.IsGrounded())
			{
				return true;
			}
			return false;
        }

        /// <summary>
        /// Locomotion은 종료 개념 X
        /// </summary>
        public override bool IsFinished(PhysicsBasedController moveSystem)
        {
            return false;
        }
		#endregion

		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		public override void Reset()
		{
            base.Reset();

            StateFlag = MoveStateFlag.Locomotive | MoveStateFlag.OnGround;

            _maxSpeed = 5;
            _maxAcceleration = 20;
            _maxDeceleration = 20;
            _friction = 8;            
		}
		public override void OnValidate()
		{
            base.OnValidate();

            Friction = _friction;
		}
        #endif
        #endregion
    }
}
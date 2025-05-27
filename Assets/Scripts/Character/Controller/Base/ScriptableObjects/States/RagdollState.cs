///스크립트 생성 일자 - 2025 - 03 - 18
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;

namespace TempNamespace.Character.Controller
{
	[CreateAssetMenu(fileName = "Ragdoll", menuName = "Character/MoveState/Ragdoll", order = 100)]
	public class RagdollState : LocomotionState
	{
        #region Inspector Fields
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Events
        void OnPhysicVolumeChanged(PhysicsVolume volume)
        {
        }
        #endregion
        
        #region Methods
        public override void OnActivated(PhysicsBasedController moveSystem)
        {
            base.OnActivated(moveSystem);
            
            moveSystem.SetRagdollState(true);
        }
        public override void OnDeactivated(PhysicsBasedController moveSystem)
        {
            base.OnDeactivated(moveSystem);

            moveSystem.SetRagdollState(false);
        }
        public override void OnStateEnter(PhysicsBasedController moveSystem, MoveStateFlag prevStateFlag)
        {
        }
        public override void OnBeforeSimulate(PhysicsBasedController character)
        {
        }

        public override void OnSimulate(PhysicsBasedController character)
        {			
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
			// if(_canSwim)
			// {
			// 	return true;
			// }
			return false;
        }

		/// <summary>
		/// 수영 불가능한 상황이라면 탈출
		/// </summary>
        public override bool IsFinished(PhysicsBasedController moveSystem)
        {
            // if(!_canSwim)
            // {
            //     return true;
            // }
			return false;
        }
		#endregion

		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		public override void Reset()
		{
            base.Reset();
			
			StateFlag = MoveStateFlag.None;
            
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
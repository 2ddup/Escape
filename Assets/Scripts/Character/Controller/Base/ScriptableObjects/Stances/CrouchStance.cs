///스크립트 생성 일자 - 2025 - 03 - 19
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using TempNamespace.Character.Controller;
using UnityEngine;

namespace TempNamespace
{
	[CreateAssetMenu(fileName = "Crouch", menuName = "Character/Stance/Crouch", order = 100)]
	public class CrouchStance : CharacterStance
	{
        #region Inspector Fields
		[SerializeField, Tooltip("앉은 상태에서의 높이")]
		private float _crouchedHeight;
        [SerializeField, Tooltip("앉은 상태에서 이동속도 계수")]
        private float _crouchedSpeedScale;
        
		/// <summary>
		/// 앉은 상태에서의 높이
		/// </summary>
		public float CrouchedHeight
		{
		   get => _crouchedHeight;
		   set => _crouchedHeight = value;
		}
        /// <summary>
        /// 앉은 상태에서 이동속도 계수
        /// </summary>
        public float CrouchedSpeedScale
        {
           get => _crouchedSpeedScale;
           set => _crouchedSpeedScale = value;
        }
        #endregion

        #region Fields
		float originalHeight = 0;
        float originalSpeedScale = 1;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void ActivateStance(PhysicsBasedController moveSystem)
        {
            base.ActivateStance(moveSystem);

			originalHeight = moveSystem.height;
            originalSpeedScale = moveSystem.SpeedMultiplier;

			moveSystem.physicsSystem.SetHeight(CrouchedHeight);
            moveSystem.SpeedMultiplier = CrouchedSpeedScale;

			StanceActivated = true;
        }
        public override void DeactivateStance(PhysicsBasedController moveSystem)
        {
            base.DeactivateStance(moveSystem);

			moveSystem.physicsSystem.SetHeight(originalHeight);
            moveSystem.SpeedMultiplier = originalSpeedScale;
            
			StanceActivated = false;
        }

        public override bool CanActivateStance(PhysicsBasedController character)
        {
            if(character.UseGrounding && character.CheckBoolKey("Crouch"))
            {
                return true;
            }
            else
                return false;
        }

        public override bool CanDeactivateStance(PhysicsBasedController character)
        {
            if(!character.UseGrounding || !character.CheckBoolKey("Crouch"))
            {
                return true;
            }
            else
                return false;
        }

        public override void Reset()
        {
            base.Reset();
			CrouchedHeight = 0.8f;
            CrouchedSpeedScale = 0.5f;
        }
        #endregion

        #region UnityEditor Only Methods
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
		#endif
        #endregion
    }
}
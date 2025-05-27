///스크립트 생성 일자 - 2025 - 03 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using TempNamespace.Character.Controller;
using TempNamespace.Character.Controller.Physics;
using TempNamespace.Character.Equipment;
using TempNamespace.Utilities;
using UnityEngine;

namespace TempNamespace.Items
{
	using Character = Character.Character;
	public class Jetpack : Equipment
	{
		#region Inspector Fields
		#endregion

		#region Fields
		Transform _transform;

		public MoveState tempState;
		#endregion
		
		#region Properties
		public new Transform transform
		{
			get
			{
				#if UNITY_EDITOR
				if(_transform == null) _transform = GetComponent<Transform>();
				#endif
				return _transform;
			}
		}

		public PhysicsBasedController Movement => EquippedCharacter == null ? null : EquippedCharacter.Controller;


        #endregion

        #region	Events
        #endregion

        #region Methods

        public override void OnEquip(Character character)
        {
			_equippedCharacter = character;
			_equippedCharacter.Controller.MoveStates.Insert(0, tempState);
        }

        public override void OnUnequip(Character character)
        {
			_equippedCharacter.Controller.MoveStates.Remove(tempState);
            _equippedCharacter = null;
        }
        /// <summary>
        /// 컴퍼넌트를 캐싱
        /// </summary>	
        protected virtual void CacheComponents()
		{
			 _transform = GetComponent<Transform>();
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
        void Update()
        {
        }
        #endregion

        #region UnityEditor Only Methods
#if UNITY_EDITOR
        protected virtual void Reset()
		{
			_equipmentType = EquipmentType.Back;
		}
		protected virtual void OnValidate()
		{
		}
		#endif
        #endregion
    }
}
///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using System;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using TempNamespace.Items;
using UnityEngine;

namespace TempNamespace.Character.Equipment
{
	[Serializable]
	public struct EquipPoint
	{
		public string pointName;
		public EquipmentType pointType;
		public Transform targetTransform;
		public IEquippable currentEquipment;
	}

	[RequireComponent(typeof(Character))]
	public class EquipmentSystem : MonoBehaviour
	{
		#region Inspector Fields
		[SerializeField, Tooltip("장비 슬롯 목록")]
		private EquipPoint[] _equipPoints;
		
		/// <summary>
		/// 장비 슬롯 목록
		/// </summary>
		public EquipPoint[] EquipPoints
		{
		   get => _equipPoints;
		   set => _equipPoints = value;
		}
		#endregion

		#region Fields
		Character _character;
		#endregion
		
		#region Properties
		#endregion

		#region	Events

		#endregion
		
		#region Methods
		protected virtual void Equip(EquipArgs args)
		{
			EquipmentType type = args.equipment.EquipmentType;
			for(int i = 0; i < EquipPoints.Length; ++i)
			{
				if(EquipPoints[i].pointType == type)
				{
					if(EquipPoints[i].currentEquipment != null)
					{
						//중복 장비 처리
					}
					else
					{
						args.equipment.transform.SetParent(EquipPoints[i].targetTransform);
						args.equipment.OnEquip(_character);

						args.equipment.transform.localPosition = Vector3.zero;
						args.equipment.transform.localEulerAngles = Vector3.zero;

						EquipPoints[i].currentEquipment = args.equipment;

						break;
					}
				}
			}
		}
		protected virtual void Unequip(EquipArgs args)
		{
			for(int i = 0; i < EquipPoints.Length; ++i)
			{
				if(EquipPoints[i].currentEquipment == args.equipment)
				{
					EquipPoints[i].currentEquipment.OnUnequip(_character);
					EquipPoints[i].currentEquipment.transform.SetParent(null);
					EquipPoints[i].currentEquipment = null;

					break;
				}
			}
		}

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_character = GetComponent<Character>();
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
        void Update()
        {
			//TODO: TEMP. 나중에 개선할 것
            if(Input.GetMouseButtonDown(0) && EquipPoints[0].currentEquipment != null)
			{
				if(EquipPoints[0].currentEquipment is IUsable usable)
				{
					usable.Use();
				}
			}
			if(Input.GetMouseButtonDown(1) && EquipPoints[1].currentEquipment != null)
			{
				if(EquipPoints[1].currentEquipment is IUsable usable)
				{
					usable.Use();
				}
			}
			//
        }

        void OnEnable()
        {
            GlobalEventManager.Instance.Subscribe<EquipArgs>("Equip", Equip);
            GlobalEventManager.Instance.Subscribe<EquipArgs>("Unequip", Unequip);
        }
        void OnDisable()
        {
            if(GlobalEventManager.IsAvailable)
			{
				GlobalEventManager.Instance.Unsubscribe<EquipArgs>("Equip", Equip);
				GlobalEventManager.Instance.Unsubscribe<EquipArgs>("Unequip", Unequip);
			}
        }
        #endregion

        #region UnityEditor Only Methods
#if UNITY_EDITOR
        protected virtual void Reset()
		{
		}
		protected virtual void OnValidate()
		{
		}
		#endif
		#endregion
	}
}
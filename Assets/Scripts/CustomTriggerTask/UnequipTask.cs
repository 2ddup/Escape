///스크립트 생성 일자 - 2025 - 03 - 31
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;
using CHG.Utilities.Triggers;
using TempNamespace.Character.Equipment;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;


namespace TempNamespace
{
	public class UnequipTask : TriggerTask
	{
		#region Inspector Fields
		[SerializeField, Tooltip("장비 해제할 아이템")]
		private Equipment _equipment;
		#endregion

		#region Properties
		/// <summary>
		/// 장비 해제할 아이템
		/// </summary>
		public Equipment Equipment
		{
		   get => _equipment;
		   set => _equipment = value;
		}
		#endregion

		#region Methods
		protected override TaskResult Execute()
		{
			GlobalEventManager.Instance.Publish("Unequip", new EquipArgs(0, Equipment));
			return TaskResult.Completed;
		}
		#endregion
	}
}
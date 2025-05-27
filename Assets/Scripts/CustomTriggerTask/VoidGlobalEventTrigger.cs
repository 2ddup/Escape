///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;
using CHG.Utilities.Triggers;
using CHG.EventDriven;


namespace TempNamespace
{
	public class VoidGlobalEventTrigger : TriggerTask
	{
		#region Inspector Fields
		[SerializeField, Tooltip("GlobalEventManager에 등록된 Event 이름")]
		private string _eventName;
		#endregion

		#region Properties
		/// <summary>
		/// GlobalEventManager에 등록된 Event 이름
		/// </summary>
		public string EventName
		{
		   get => _eventName;
		   set => _eventName = value;
		}
		#endregion

		#region Methods
		protected override TaskResult Execute()
		{
			GlobalEventManager.Instance.Publish(EventName);

			return TaskResult.Completed;
		}
		#endregion
	}
}
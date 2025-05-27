///스크립트 생성 일자 - 2025 - 03 - 10
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using CHG.Utilities.Triggers;
using UnityEngine.Events;


namespace CHG
{
	public class InvokeTask : TriggerTask
	{
		#region Inspector Fields
		[SerializeField, Tooltip("발생시킬 이벤트")]
		private UnityEvent _targetEvent;
		#endregion

		#region Properties
		/// <summary>
		/// 발생시킬 이벤트
		/// </summary>
		public UnityEvent TargetEvent
		{
			get => _targetEvent;
			set => _targetEvent = value;
		}
		#endregion

		#region Methods
		protected override TaskResult Execute()
		{
			TargetEvent?.Invoke();
			return TaskResult.Completed;
		}
		#endregion
	}
}
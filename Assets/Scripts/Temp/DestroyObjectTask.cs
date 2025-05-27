///스크립트 생성 일자 - 2025 - 03 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using CHG.Utilities.Triggers;


namespace TempNamespace
{
	public class DestroyObjectTask : TriggerTask
	{
		#region Inspector Field
		[SerializeField, Tooltip("파괴할 GameObject")]
		private GameObject _targetObject;
		#endregion

		#region Methods
		protected override TaskResult Execute()
		{
			Destroy(_targetObject);
			return TaskResult.Completed;
		}
		#endregion
	}
}
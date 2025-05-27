///스크립트 생성 일자 - 2025 - 03 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;
using CHG.Utilities.Triggers;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;


namespace TempNamespace
{
	public class ChangePerspectiveTask : TriggerTask
	{
		public Character.Controller.DynamicPerspectiveController.ControllerPerspective perspective;

		#region Methods
		protected override TaskResult Execute()
		{
			GlobalEventManager.Instance.Publish("ChangePerspective", new ChangePerspectiveArgs(perspective));

			return TaskResult.Completed;
		}
		#endregion
	}
}
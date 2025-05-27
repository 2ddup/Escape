///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;
using CHG.Utilities.Triggers;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;


namespace TempNamespace
{
	public class ChangeCameraFocusTask : TriggerTask
	{
		#region Inspector Fields
		[SerializeField, Tooltip("새로운 Camera 목표 Transform")]
		private Transform _newFocusTransform;
		
		/// <summary>
		/// NEW CAMERA IS HERE
		/// </summary>
		public Transform NewFocusTransform
		{
		   get => _newFocusTransform;
		   set => _newFocusTransform = value;
		}
		#endregion

		#region Properties
		#endregion

		#region Methods
		protected override TaskResult Execute()
		{
			GlobalEventManager.Instance.Publish("FocusCameraTo", new ChangeCameraFocusArgs(NewFocusTransform));

			return TaskResult.Completed;
		}
		#endregion
	}
}
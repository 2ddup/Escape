///스크립트 생성 일자 - 2025 - 03 - 05
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using UnityEditor;

namespace TempNamespace
{
	[CustomEditor(typeof(CameraController))]
	public class CameraControllerEditor : UnityEditor.Editor
	{
		CameraController targetObject;
		
		void OnEnable()
		{
			//targetObject = serializedObject.Get
		}
		void OnDisable()
		{
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
}
///스크립트 생성 일자 - 2025 - 03 - 05
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using TempNamespace.Character.Controller;
using UnityEngine;

namespace TempNamespace
{
	[RequireComponent(typeof(Camera))]
	public class CameraController : MonoBehaviour
	{
		#region Member Enums
		public enum CameraState
		{
			/// <summary>
			/// 이동 정지
			/// </summary>
			None,
			/// <summary>
			/// 특정 Transform과 동기화중
			/// </summary>
			SyncTransform,
			/// <summary>
			/// 미리 설정된 Path를 따라서 이동중
			/// </summary>
			FollowPath,
			/// <summary>
			/// 특정 객체에 Focus를 맞추고 RotateAround중
			/// </summary>
			FocusObject
		}
		#endregion

		#region Inspector Fields
		[SerializeField, Tooltip("카메라 추적 상태")]
		CameraState currentState;

		[SerializeField, Tooltip("동기화 Transform")]
		private Transform targetTransform;
		[SerializeField, Tooltip("즉시 동기화")]
		private bool instantSync;

		[SerializeField, Tooltip("위치 동기화 속도")]
		private float smoothPositionSpeed = 50;
		[SerializeField, Tooltip("회전 동기화 속도")]
		private float smoothRotationSpeed = 50;
		#endregion

		#region Fields
		Transform _transform;
		Camera _camera;
		#endregion
		
		#region Properties
		public new Camera camera
		{
			get
			{
				#if UNITY_EDITOR
				if(_camera == null) _camera = GetComponent<Camera>();
				#endif
				return _camera;
			}
		}
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
		
		public Transform TargetTransform
		{
			get { return targetTransform; }
			set { targetTransform = value; }
		}
		#endregion
		
		#region Methods
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			 _transform = GetComponent<Transform>();
			 _camera = GetComponent<Camera>();
		}

		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}

        void LateUpdate()
        {
			
            if(currentState == CameraState.SyncTransform)
			{
				if(!instantSync)
				{
					transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime*smoothPositionSpeed);
					transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime*smoothRotationSpeed);
				}
				else
					transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
			}
        }

        void Reset()
        {
            smoothPositionSpeed = 50;
			smoothRotationSpeed = 50;
        }
        void OnValidate()
        {
            smoothPositionSpeed = Math.Max(smoothPositionSpeed, 0);
			smoothRotationSpeed = Mathf.Max(smoothRotationSpeed, 0);
        }
        #endregion
    }
}
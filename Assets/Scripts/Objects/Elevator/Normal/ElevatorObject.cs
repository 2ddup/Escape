///스크립트 생성 일자 - 2025 - 03 - 10
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System.Collections.Generic;
using UnityEngine;

namespace TempNamespace.Objects
{
	public class ElevatorObject : MonoBehaviour
	{
		#region Member Struct
		/// <summary>
		/// 엘리베이터가 멈춰설 층의 데이터
		/// </summary>
		[System.Serializable]
		public struct Floor
		{
			/// <summary>
			/// 층의 이름
			/// </summary>
			public string floorName;
			/// <summary>
			/// 층의 위치
			/// </summary>
			public float position;
		}
		#endregion

		#region Inspector Fields
		[SerializeField, Tooltip("엘리베이터가 운행하는 층의 데이터")]
		private List<Floor> _floors;
		
		/// <summary>
		/// 엘리베이터가 운행하는 층의 데이터
		/// </summary>
		public List<Floor> Floors
		{
			get => _floors;
			set => _floors = value;
		}
		#endregion

		#region Fields
		Transform _transform;
		Animator doorAnimator;
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
		#endregion
		
		#region Methods
		[ContextMenu("Open Elevator Door")]
		public void OpenDoor()
		{
			SoundManager.Instance.PlaySFX("Elevator Open", false);
			doorAnimator.Play("Open");
		}

		[ContextMenu("Close Elevator Door")]
		public void CloseDoor()
		{
			SoundManager.Instance.PlaySFX("Elevator Close", false);
			doorAnimator.Play("Close");
		}

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			doorAnimator = GetComponentInChildren<Animator>();
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
		#endregion

		#region UnityEditor Only Methods
		protected virtual void Reset()
		{

		}
		protected virtual void OnValidate()
		{
			
		}
		#endregion
	}
}
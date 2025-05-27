///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace
{
	public class EventCounter : MonoBehaviour
	{
		#region Inspector Fields

		[SerializeField, Tooltip("초기 카운트")]
		int _initialCount;

		[SerializeField, Tooltip("목표 카운트"), Min(1)]
		private int _targetCount;
		[SerializeField, Tooltip("카운트 도달시 이벤트")]
		private UnityEvent _onCountReached;
		
		
		#endregion

		#region Fields
		Transform _transform;
		private int _currentCount;
		
		public int CurrentCount
		{
			get => _currentCount;
			set 
			{
				_currentCount = value;
				if(_currentCount == _targetCount)
				{
					OnCountReached?.Invoke();
				}
			}
		}
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
		/// <summary>
		/// 목표 카운트
		/// </summary>
		public int TargetCount
		{
		   get => _targetCount;
		   set => _targetCount = value;
		}
		/// <summary>
		/// 카운트 도달시 이벤트
		/// </summary>
		public UnityEvent OnCountReached
		{
		   get => _onCountReached;
		   set => _onCountReached = value;
		}
		#endregion

		#region	Events

		#endregion
		
		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
			_currentCount = _initialCount;
		}
		#endregion

		#region Methods
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			 _transform = GetComponent<Transform>();
		}

		public void AddCount()
		{
			AddCount(1);
		}
		
		public void AddCount(int value)
		{
			CurrentCount += value;
		}

		public void MinusCount()
		{
			MinusCount(1);
		}
		public void MinusCount(int value)
		{
			CurrentCount -= value;
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
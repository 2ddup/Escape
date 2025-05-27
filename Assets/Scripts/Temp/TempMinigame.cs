///스크립트 생성 일자 - 2025 - 03 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using CHG.EventDriven;
using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace
{
	public class TempMinigame : MonoBehaviour
	{
		#region Inspector Fields
		public bool isFocused = false;
		public UnityEvent OnFocused;
		public UnityEvent OnUnfocused;
		#endregion

		#region Fields
		Transform _transform;
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

		#region	Events
		public void Focus()
		{
			isFocused = true;
			OnFocused?.Invoke();
		}

		#endregion
		
		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}

		void Update()
		{
			if(isFocused)
			{
				if(Input.GetKeyDown(KeyCode.Escape))
				{
					GlobalEventManager.Instance.Publish("ResumeCharacter");
					GlobalEventManager.Instance.Publish("FocusToCharacter");

					OnUnfocused?.Invoke();

					isFocused = false;
				}
			}
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
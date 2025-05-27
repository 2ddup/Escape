///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using TempNamespace.InteractableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace
{
	public class CameraFocusInteractable : MonoBehaviour, IInteractable
	{
		#region Inspector Fields
		[SerializeField, Tooltip("이 Interactor의 설명 Text")]
		private string _tooltipText;
		[SerializeField, Tooltip("상호작용 가능 상태인가?")]
		private bool _isInteractable = true;

		[SerializeField, Tooltip("Camera Focus Transform")]
		private Transform _focusTransform;

		[SerializeField, Tooltip("Camera Focus를 얻었을 때")]
		private UnityEvent _onFocusIn;
		[SerializeField, Tooltip("Player에게 Focus가 돌아갔을 때")]
		private UnityEvent _onFocusOut;
		
		
		
		#endregion

		#region Fields
		Transform _transform;
		private bool _isInteracting;
		private bool _isFocused;
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
		/// Camera Focus Transform
		/// </summary>
		public Transform FocusTransform
		{
		   get => _focusTransform;
		   set => _focusTransform = value;
		}
		/// <summary>
		/// Camera Focus를 얻었을 때
		/// </summary>
		public UnityEvent OnFocused
		{
		   get => _onFocusIn;
		   set => _onFocusIn = value;
		}
		/// <summary>
		/// Player에게 Focus가 돌아갔을 때
		/// </summary>
		public UnityEvent OnFocusOut
		{
		   get => _onFocusOut;
		   set => _onFocusOut = value;
		}
        
		/// <summary>
		/// 상호작용 가능 상태인가?
		/// </summary>
		public bool IsInteractable
		{
		   get => _isInteractable;
		   set => _isInteractable = value;
		}
		/// <summary>
		/// 현재 상호작용중인가?
		/// </summary>
		public bool IsInteracting
		{
			get => _isInteracting;
			set => _isInteracting = value;
		}
		/// <summary>
		/// 이 Interactor의 설명 Text
		/// </summary>
		public string TooltipText
		{
		   get => _tooltipText;
		   set => _tooltipText = value;
		}
        #endregion

        #region	Events

        #endregion

        #region MonoBehaviour Methods
        protected virtual void Awake()
		{
			CacheComponents();
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
		public virtual bool Interact(GameObject interactor)
        {
			if(!IsInteractable)
				return false;
			else
			{
				if(IsInteracting && !_isFocused)
					return false;

				if(IsInteracting)
				{
					SetFocusToPlayer();
				}
				else
				{
					GetFocusOfCamera();
				}

				return true;
			}
        }
		public void GetFocusOfCamera()
		{
			IsInteracting = true;
			_isFocused = true;

			GlobalEventManager.Instance.Publish("PauseCharacterControl");
			GlobalEventManager.Instance.Publish("FocusCameraTo", new ChangeCameraFocusArgs(FocusTransform));
			OnFocused?.Invoke();

			Unfocus();
		}
		public void SetFocusToPlayer()
		{
			IsInteracting = false;
			_isFocused = false;

			GlobalEventManager.Instance.Publish("ResumeCharacterControl");
			GlobalEventManager.Instance.Publish("FocusToCharacter");			
		}

        public void Focus()
        {
			if(IsInteractable && !IsInteracting)
            	GlobalEventManager.Instance.Publish("FocusInteractor", new InteractorMessageArgs(TooltipText));
        }

        public void Unfocus()
        {
			if(GlobalEventManager.IsAvailable)
				GlobalEventManager.Instance.Publish("UnfocusInteractor");
        }
		#endregion

		
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected virtual void Reset()
		{
			_isInteractable = true;
			_isInteracting = false;
			_isFocused = false;
		}
		protected virtual void OnValidate()
		{
		}
		#endif
        #endregion
    }
}
///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace.InteractableObjects
{
	[RequireComponent(typeof(Collider))]
	public class SimpleInteractable : MonoBehaviour, IInteractable
	{
		[SerializeField]
		protected string messageText;
		[SerializeField]
		protected bool _isInteractable;
		[SerializeField, Tooltip("현재 상호작용중인가?")]
		protected bool _isInteracting;
		
		public UnityEvent OnInteract;
		public UnityEvent<GameObject> OnInteractWithInteractor;

		new Collider collider;
		
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


		void Awake()
		{
			CacheComponents();
		}
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			collider = GetComponent<Collider>();
		}

        public bool Interact(GameObject interactor)
        {
			if(!IsInteractable)
				return false;

            OnInteract?.Invoke();
			OnInteractWithInteractor?.Invoke(interactor);
			IsInteracting = true;			

			return true;
        }

        public void Focus()
        {
			if(IsInteractable)
            	GlobalEventManager.Instance.Publish("FocusInteractor", new InteractorMessageArgs(messageText));
        }

        public void Unfocus()
        {
			if(GlobalEventManager.IsAvailable)
				GlobalEventManager.Instance.Publish("UnfocusInteractor");
        }
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
        void OnDrawGizmos()
        {
			
        }
        void OnValidate()
        {
            //if(layer)
        }
#endif
        #endregion
    }
}
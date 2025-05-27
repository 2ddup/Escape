///스크립트 생성 일자 - 2025 - 03 - 04
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using UnityEngine;

namespace TempNamespace.ObjectDrag
{
	[RequireComponent(typeof(Rigidbody), typeof(Collider))]
	public class GrabbableObject : MonoBehaviour, IGrabbable
	{	
		#region Inspector Properties
		[SerializeField, Tooltip("Grab 가능한 상태인가?")]
		protected bool _isInteractable;		
		#endregion


		#region Fields
		protected Transform _transform;
		protected Rigidbody _rigidbody;
		protected bool _isInteracting;
		private bool _isDragging;
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
		public new Rigidbody rigidbody
		{
			get
			{
				#if UNITY_EDITOR
				if(_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
				#endif
				return _rigidbody;
			}
		}
		/// <summary>
		/// 잡을 수 있는 상태인가?
		/// </summary>
        public bool IsInteractable
		{
			get => _isInteractable;
			set => _isInteractable = value;
		}

		/// <summary>
		/// 이미 누군가 잡고 있는 상태인가?
		/// </summary>
        public bool IsInteracting
		{
			get => _isInteracting;
			set => _isInteracting = value;
		}
		/// <summary>
		/// 드래그 중인가?
		/// </summary>
		public bool IsDragging
		{
			get => _isDragging;
			set => _isDragging = value;
		}
        #endregion

        #region Methods
        /// <summary>
        /// 컴퍼넌트를 캐싱
        /// </summary>	
        protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			_rigidbody = GetComponent<Rigidbody>();
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
        void Update()
        {
            
        }

        public virtual void Focus()
        {
			if(IsInteractable)
            	GlobalEventManager.Instance.Publish("FocusInteractor", new InteractorMessageArgs("E 옮기기"));
        }

        public virtual void Unfocus()
        {
			GlobalEventManager.Instance.Publish("UnfocusInteractor");
        }

        public virtual bool Interact(GameObject grabber)
        {
			rigidbody.useGravity = false;
			rigidbody.drag = 10;
			rigidbody.angularDrag = 30;
			_isInteracting = true;
			_isDragging = true;

			return true;
        }

        public virtual bool Release(GameObject grabber)
        {
			rigidbody.useGravity = true;
			rigidbody.drag = 0;
			rigidbody.angularDrag = 0;
			_isInteracting = false;
			_isDragging = false;

            return true;
        }
		#endregion
		

        #region UnityEditor Only Methods
		#if UNITY_EDITOR
        protected virtual void Reset()
        {
			_isInteractable = true;
			_isInteracting = false;
        }
		protected virtual void OnValidate()
		{
		}
		#endif
        #endregion
    }
}
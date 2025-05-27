///스크립트 생성 일자 - 2025 - 03 - 05
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;

namespace TempNamespace.InteractableObjects
{
	public class RayInteractor : MonoBehaviour
	{
		#region Inspector Fields
		[SerializeField, Tooltip("Interact를 발생시킬 Key Code")]
        protected KeyCode interactKey = KeyCode.Mouse0;
		[SerializeField, Tooltip("상호작용 가능 레이어")]
		LayerMask layerMask;
		[SerializeField, Tooltip("인터랙터션 최대 거리")]
		protected float searchDistance;
		#endregion

		#region Fields
		protected Transform _transform;
        protected Camera cam;
        protected RaycastHit hitInfo;
		protected IInteractable currentTargetInteractor;
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
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			cam = Camera.main;
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
        void Update()
        {
            if(Input.GetKeyDown(interactKey))
            {
				if(currentTargetInteractor != null)
				{
					currentTargetInteractor.Interact(this.gameObject);
				}
            }
        }
        void FixedUpdate()
        {
            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, searchDistance, layerMask))
			{
				IInteractable interactor = hitInfo.collider.gameObject.GetComponent<IInteractable>();
				
				if(currentTargetInteractor != null && 
					(currentTargetInteractor != interactor || !currentTargetInteractor.IsInteractable))
				{
					currentTargetInteractor.Unfocus();
				}

				interactor?.Focus();
				currentTargetInteractor = interactor;
			}
			else
			{
				if(currentTargetInteractor != null)
				{
					currentTargetInteractor.Unfocus();
					currentTargetInteractor = null;
				}
			}
        }

        void OnDisable()
        {
            if(currentTargetInteractor != null)
			{
				currentTargetInteractor.Unfocus();
				currentTargetInteractor = null;
			}
        }
        void Reset()
        {
            interactKey = KeyCode.Mouse0;
			searchDistance = 5;
        }
        void OnDrawGizmos()
        {
			#if UNITY_EDITOR
			if(cam == null) cam = Camera.main;
			#endif
            Gizmos.color = Color.red;
			Gizmos.DrawLine(cam.transform.position, cam.transform.position + (cam.transform.forward*searchDistance));
        }
        #endregion
    }
}
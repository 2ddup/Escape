///스크립트 생성 일자 - 2025 - 03 - 31
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using System;
using UnityEngine;

namespace TempNamespace.ObjectDrag
{
	public class ObjectGrabber : MonoBehaviour
	{		
		#region Inspector Fields
		[Header("Drag")]
		[SerializeField, Tooltip("Grab을 발생시킬 Key Code")]
        protected KeyCode interactKey = KeyCode.Mouse0;
		[SerializeField, Tooltip("상호작용 가능 레이어")]
		LayerMask layerMask;
		[SerializeField, Tooltip("최대 탐색 거리")]
		protected float searchDistance;
		[SerializeField, Tooltip("드래그 중 목표 소실 완충 시간")]
		private float _tolerance;
		

		//TODO: TEMP
		[Header("Force")]
		public Transform dragOrigin;
		public float forceStrength = 10f;      // 주 끌어당기는 힘의 세기
		public float minDistance = 0.5f;       // 목표 도착으로 간주할 최소 거리 (약간의 여유)
		public float arrivalDistance = 3.0f;   // 제동 또는 힘 조절을 시작할 거리 (minDistance보다 커야 함)
		public float brakingFactor = 0.8f;     // 제동력 계수 (클수록 강하게 제동)
		public float arrivalDamping = 0.95f;   // 도착 구간에서의 속도 감쇠율 (1에 가까울수록 천천히 감속)
		public float baseDrag = 0.5f;          // 기본 Rigidbody Drag 값
		public float arrivalDrag = 2.0f;       // 도착 구간에서 적용할 Drag 값 (선택적)
		#endregion

		#region Fields
		protected Transform _transform;
        protected Camera _camera;
		protected float _lostTime;
        protected RaycastHit hitInfo;
		protected IGrabbable focusedObject;
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
		/// 드래그 중 목표 소실 완충 시간
		/// </summary>
		public float Tolerance
		{
		   get => _tolerance;
		   set => _tolerance = value;
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
		

        void FixedUpdate()
        {
			if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hitInfo, searchDistance, layerMask))
			{
				IGrabbable grabbable = hitInfo.collider.gameObject.GetComponent<IGrabbable>();
				
				if((focusedObject == null || !focusedObject.IsDragging) && grabbable.IsInteractable && !grabbable.IsInteracting)
				{
					focusedObject?.Unfocus();
					grabbable?.Focus();
					focusedObject = grabbable;
				}
				else if(focusedObject != null && focusedObject.IsInteracting && !focusedObject.IsDragging)
				{
					focusedObject?.Unfocus();
					focusedObject = null;
				}
				else if(focusedObject != null && focusedObject.IsDragging)
				{
					_lostTime = 0;
				}
			}
			else
			{
				if(focusedObject != null)
				{
					if(focusedObject.IsDragging && _lostTime < Tolerance)
					{
						_lostTime += Time.fixedDeltaTime;
						
					}
					else
					{
						focusedObject.Unfocus();
						if(focusedObject.IsDragging)
						{
							focusedObject.Release(this.gameObject);
						}
						focusedObject = null;
					}
				}
			}

			if(focusedObject != null && focusedObject.IsDragging)
			{
				DragObject();
			}
		}
		
        void Update()
        {
            if(Input.GetKeyDown(interactKey))
            {
				if(focusedObject != null)
				{
					focusedObject.Unfocus();
					focusedObject.Interact(this.gameObject);

					if(focusedObject.transform.TryGetComponent(out SyncGrabbable sync))
					{
						sync.TakeOwnership(this);
					}
				}
            }
			else if(focusedObject != null)
			{
				if(Input.GetKeyUp(interactKey))
				{
					focusedObject.Release(this.gameObject);
					focusedObject = null;
				}
			}
        }

        void OnDisable()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// 컴퍼넌트를 캐싱
        /// </summary>	
        protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			_camera = Camera.main;

			if(dragOrigin == null)
				dragOrigin = transform;
		}

        private void DragObject()
        {
			Vector3 currentPosition = focusedObject.rigidbody.position;
			Vector3 targetPosition = dragOrigin.position;
			float distance = Vector3.Distance(targetPosition, currentPosition);
			Vector3 directionToTarget = (targetPosition - currentPosition).normalized;

			// 1. 도착 구간 처리 (minDistance 이내)
			if (distance <= minDistance)
			{
				// 속도를 점진적으로 0에 가깝게 만듦 (진동 방지)
				focusedObject.rigidbody.velocity *= (1.0f - arrivalDamping * Time.fixedDeltaTime * 10f); // 프레임 속도에 덜 민감하게 조정

				// 또는 더 강하게 멈추려면:
				// focusedObjectRigidbody.velocity = Vector3.Lerp(focusedObjectRigidbody.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);

				// 도착 시 Drag를 높여 안정화 (선택적)
				focusedObject.rigidbody.drag = arrivalDrag;

				// 이 구간에서는 추가적인 힘을 가하지 않음
				return; // 아래 로직 실행 방지
			}

			// 도착 구간이 아니라면 기본 Drag로 복구 (선택적)
			focusedObject.rigidbody.drag = baseDrag;


			// 2. 제동 구간 처리 (arrivalDistance 이내, minDistance 바깥)
			if (distance < arrivalDistance)
			{
				// 목표를 향하는 기본 힘 적용 (선택적으로 여기서 힘을 줄일 수도 있음)
				Vector3 pullForce = directionToTarget * forceStrength * (distance / arrivalDistance); // 거리에 비례해 힘 감소
				focusedObject.rigidbody.AddForce(pullForce, ForceMode.Force);

				// 현재 속도의 반대 방향으로 제동력 적용 (오버슈팅 방지 및 감속)
				Vector3 brakingForce = -focusedObject.rigidbody.velocity * brakingFactor;
				focusedObject.rigidbody.AddForce(brakingForce, ForceMode.Force);
			}
			// 3. 주 이동 구간 처리 (arrivalDistance 바깥)
			else
			{
				// 최대 힘으로 목표 지점까지 끌어당김
				Vector3 force = directionToTarget * forceStrength;
				focusedObject.rigidbody.AddForce(force, ForceMode.Force);
			}
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
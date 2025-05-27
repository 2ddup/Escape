///스크립트 생성 일자 - 2025 - 03 - 31
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using System;
using UnityEngine;

namespace TempNamespace
{
	public class RagdollController : MonoBehaviour
	{
		#region Inspector Fields
		[SerializeField, Tooltip("연결된 애니메이터")]
		private Animator _animator;
		[SerializeField, Tooltip("래그돌 파트")]
		private GameObject[] _ragdollParts;
		
		/// <summary>
		/// 래그돌 파트
		/// </summary>
		public GameObject[] RagdollParts
		{
		   get => _ragdollParts;
		   set => _ragdollParts = value;
		}
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
		
		/// <summary>
		/// 연결된 애니메이터
		/// </summary>
		public Animator Animator
		{
		   get => _animator;
		   set => _animator = value;
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

        public void SetRagdoll(bool isRagdollActivate)
        {
            if(isRagdollActivate)
				ActivateRagdoll();
			else
				DeactivateRagdoll();
        }
		public void ActivateRagdoll()
		{
			Animator.enabled = false;
			for(int i = 0; i < RagdollParts.Length; ++i)
			{
				RagdollParts[i].GetComponent<Collider>().enabled = true;
				RagdollParts[i].GetComponent<Rigidbody>().isKinematic = false;
			}
		}
		public void DeactivateRagdoll()
		{
			Animator.enabled = true;
			for(int i = 0; i < RagdollParts.Length; ++i)
			{
				RagdollParts[i].GetComponent<Collider>().enabled = false;
				RagdollParts[i].GetComponent<Rigidbody>().isKinematic = true;
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
///스크립트 생성 일자 - 2025 - 04 - 03
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using System;
using TempNamespace.InteractableObjects;
using TempNamespace.ObjectDrag;
using UnityEngine;

namespace TempNamespace
{
	[RequireComponent(typeof(IGrabbable), typeof(PhotonView))]
	public class SyncGrabbable : MonoBehaviour
	{
		#region Inspector Fields
		private IGrabbable _grabbable;

		#endregion

		#region Fields
		Transform _transform;
		Vector3 position;
		Quaternion rotation;

		private PhotonView _view;
		
		public PhotonView View
		{
			get => _view;
			set => _view = value;
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
		/// 동기화할 인터랙터
		/// </summary>
		public IGrabbable Grabbable
		{
			get => _grabbable;
			set => _grabbable = value;
		}
		#endregion

		#region	Events
		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if(stream.isWriting)
			{
				stream.SendNext(transform.position);
				stream.SendNext(transform.rotation);

				stream.SendNext(Grabbable.IsInteractable);
				stream.SendNext(Grabbable.IsInteracting);
			}
			else
			{
				position = (Vector3)stream.ReceiveNext();
				rotation = (Quaternion)stream.ReceiveNext();

				Grabbable.IsInteractable = (bool)stream.ReceiveNext();
				Grabbable.IsInteracting = (bool)stream.ReceiveNext();
			}
		}
        internal void TakeOwnership(ObjectGrabber objectGrabber)
        {
            if(!View.isMine)
			{
				View.TransferOwnership(PhotonNetwork.player);
			}
        }
		#endregion
		
		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();

			position = transform.position;
			rotation = transform.rotation;
		}
		#endregion

		#region Methods
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			Grabbable = GetComponent<IGrabbable>();
			View = GetComponent<PhotonView>();
		}
        void Update()
        {
			if(!View.isMine)
			{
				transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime*5);
				transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime*5);
			}
        }
        void FixedUpdate()
        {
			//TODO: 무조건 고칠 것
            Grabbable.transform.GetComponent<Rigidbody>().isKinematic = !View.isMine;
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
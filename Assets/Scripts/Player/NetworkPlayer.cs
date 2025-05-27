///스크립트 생성 일자 - 2025 - 04 - 02
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using System;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using UnityEngine;

namespace TempNamespace.Player
{
	using Character = Character.Character;

	[RequireComponent(typeof(PhotonView))]
	public class PhotonNetworkPlayer : GamePlayer
	{
		#region Inspector Fields
		[SerializeField, Tooltip("연결된 Photon View Class")]
		private PhotonView _view;
		
		/// <summary>
		/// 연결된 Photon View Class
		/// </summary>
		public PhotonView View
		{
		   get => _view;
		   set => _view = value;
		}
		#endregion

		#region Fields
		private Vector3 position;
		private Quaternion rotation;
        #endregion

        #region Properties
        public override Transform transform
		{
			get
			{
				if(PlayerCharacter != null)
				{
					return PlayerCharacter.transform;
				}
				else
					return base.transform;
			}
		}

		/// <summary>
		/// 내 소유인가?
		/// </summary>
		public bool IsMine
		{
			get
			{
				#if UNITY_EDITOR
				if(View == null) View = GetComponent<PhotonView>();
				#endif
				return View.isMine;
			}
		}
		#endregion

		#region	Events
		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if(stream.isWriting)
			{
				stream.SendNext(transform.position);
				stream.SendNext(transform.rotation);

				stream.SendNext(PlayerCharacter.isDead);
			}
			else
			{
				position = (Vector3)stream.ReceiveNext();
				rotation = (Quaternion)stream.ReceiveNext();

				PlayerCharacter.isDead = (bool)stream.ReceiveNext();
			}
		}
		#endregion
		
		#region Methods        
		private void AvatarMode()
        {
			//TODO: 제거를 Factory 패턴을 통한 모듈 조립 방식으로 대체할 것
			Destroy(PlayerCharacter.Equipment);
			Destroy(CameraController.gameObject);

			PlayerCharacter.Controller.Pause(true);
			PlayerCharacter.InputHandler.enabled = false;
        }
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected override void CacheComponents()
		{
			base.CacheComponents();
			View = GetComponent<PhotonView>();
		}
        #endregion

        #region MonoBehaviour Methods
        protected override void Awake()
        {
            base.Awake();
        }
        void Start()
        {
			if(!IsMine)
			{
				AvatarMode();
			}            
        }
        //TODO: Avatar Character 따로 만들어서 거기서 처리할 것
        protected virtual void Update()
		{
			if(!IsMine)
			{
				transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime*5);
				transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime*5);
			}
		}

        protected override void OnEnable()
        {
			if(IsMine)
			{
				GlobalEventManager.Instance.Subscribe("PauseCharacterControl", PauseControl);
				GlobalEventManager.Instance.Subscribe("ResumeCharacterControl", ResumeControl);

				GlobalEventManager.Instance.Subscribe("FocusToCharacter", FocusToCharacter);

				GlobalEventManager.Instance.Subscribe<ChangeCameraFocusArgs>("FocusCameraTo", ChangeCameraFocus);
			}
        }
        protected override void OnDisable()
        {
            if(IsMine && GlobalEventManager.IsAvailable)
			{
				GlobalEventManager.Instance.Unsubscribe("PauseCharacterControl", PauseControl);
				GlobalEventManager.Instance.Unsubscribe("ResumeCharacterControl", ResumeControl);

				GlobalEventManager.Instance.Unsubscribe("FocusToCharacter", FocusToCharacter);

				GlobalEventManager.Instance.Unsubscribe<ChangeCameraFocusArgs>("FocusCameraTo", ChangeCameraFocus);
			}
        }
        #endregion

        #region UnityEditor Only Methods
		#if UNITY_EDITOR

		#endif
		#endregion
	}
}
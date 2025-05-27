///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using UnityEngine;

namespace TempNamespace.Player
{
	using Character = Character.Character;
	public class GamePlayer : MonoBehaviour
	{
		#region Inspector Fields
		[SerializeField, Tooltip("플레이어의 고유 ID 값")]
		protected int _playerID;
		[SerializeField, Tooltip("이 Player와 연결된 Character 객체")]
		protected Character _playerCharacter;
		[SerializeField, Tooltip("이 Player가 사용하는 Camera 객체")]
		protected CameraController _cameraController;
		
		/// <summary>
		/// 이 Player가 사용하는 Camera 객체
		/// </summary>
		public CameraController CameraController
		{
		   get => _cameraController;
		   set => _cameraController = value;
		}
		
		/// <summary>
		/// 이 Player와 연결된 Character 객체
		/// </summary>
		public Character PlayerCharacter
		{
		   get => _playerCharacter;
		   set => _playerCharacter = value;
		}
		#endregion

		#region Fields
		protected Transform _transform;
		#endregion
		
		#region Properties
		public virtual new Transform transform
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
		/// 플레이어의 고유 ID 값
		/// </summary>
		public int PlayerID
		{
		   get => _playerID;
		   set => _playerID = value;
		}
		#endregion

		#region	Events
		protected virtual void PauseCharacter()
		{
			PlayerCharacter.PauseMovement(true);
		}
		protected virtual void PauseControl()
		{
			PlayerCharacter.PauseControl(true);
		}
		protected virtual void ResumeCharacter()
		{
			PlayerCharacter.PauseMovement(false);
		}
		protected virtual void ResumeControl()
		{
			PlayerCharacter.PauseControl(false);
		}
		protected virtual void ChangeCameraFocus(ChangeCameraFocusArgs args)
		{
			CameraController.TargetTransform = args.newTarget;
		}
		protected virtual void FocusToCharacter()
		{
			CameraController.TargetTransform = PlayerCharacter.Controller.CameraHolder;
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

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}

        protected virtual void OnEnable()
        {
            GlobalEventManager.Instance.Subscribe("PauseCharacterControl", PauseControl);
			GlobalEventManager.Instance.Subscribe("ResumeCharacterControl", ResumeControl);

			GlobalEventManager.Instance.Subscribe("FocusToCharacter", FocusToCharacter);

			GlobalEventManager.Instance.Subscribe<ChangeCameraFocusArgs>("FocusCameraTo", ChangeCameraFocus);
        }
        protected virtual void OnDisable()
        {
            if(GlobalEventManager.IsAvailable)
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
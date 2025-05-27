///스크립트 생성 일자 - 2025 - 03 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using TempNamespace.Character.Controller;
using CHG.EventDriven;
using DG.Tweening;
using TempNamespace.EventDriven.Arguments;
using TempNamespace.Character.Equipment;
using Unity.VisualScripting.FullSerializer.Internal;

namespace TempNamespace.Character
{
	/// <summary>
	/// 캐릭터 객체의 최상위 시스템
	/// 이 클래스에 하위 시스템들을 연결해 캐릭터를 구현
	/// </summary>
	public class Character : MonoBehaviour
	{
		#region Inspector Fields
		[SerializeField, Tooltip("컨트롤 처리를 담당할 시스템 클래스(Nullable)")]
		private PhysicsBasedController _controller;
		[SerializeField, Tooltip("캐릭터의 입력을 처리하는 클래스")]
		private CharacterInputHandler _inputHanlder;
		
		/// <summary>
		/// 캐릭터의 입력을 처리하는 클래스
		/// </summary>
		public CharacterInputHandler InputHandler
		{
		   get => _inputHanlder;
		   set => _inputHanlder = value;
		}
		[SerializeField, Tooltip("아이템 장비 시스템(TODO: 임시 시스템, 추후 개선할 것)")]
		private EquipmentSystem _equipment;

		
		#endregion

		#region Fields
		Transform _transform;
		bool _isRespawning = false;
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
		/// 이동 처리를 담당할 시스템 클래스(Nullable)
		/// </summary>
		public PhysicsBasedController Controller
		{
			get => _controller;
			set => _controller = value;
		}
		
		/// <summary>
		/// 아이템 장비 시스템(TODO: 임시 시스템, 추후 개선할 것)
		/// </summary>
		public EquipmentSystem Equipment
		{
			get => _equipment;
			set => _equipment = value;
		}
		public bool isDead = false;
		/// <summary>
		/// 재생성중인가?
		/// </summary>
		public bool IsRespawning
		{
			get => _isRespawning;
			set => _isRespawning = value;
		}

		#endregion

		#region	Events
		
		#endregion
		
		#region Methods

		/// <summary>
		/// 이동 기능을 일시정지
		/// </summary>
		/// <param name="isPause">true일 경우 일시정지, false일 경우 재개</param>
		/// <param name="clearState">현재 이동 데이터를 초기화</param>
		public void PauseMovement(bool isPause, bool clearState = true)
		{
			Controller.Pause(isPause, clearState);
			InputHandler.enabled = !isPause;
		}
		public void PauseControl(bool isPause, bool clearState = true)
		{
			if(clearState)
				Controller.ClearState();
			InputHandler.enabled = !isPause;
		}

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			if(_controller == null)
                _controller = GetComponentInChildren<Controller.PhysicsBasedController>();
			
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
		void TempReverseGravity()
		{
			Controller.gravityScale *= -1;
			Controller.PauseGroundConstraint(0.15f);

			Vector3 current = Controller.transform.localEulerAngles;
			current.x += 180;
			Controller.transform.DOLocalRotate(current, 1.0f);
		}
		void TempRagdoll(RagdollArgs args)
		{
			if(args.character == this)
			{
				Controller.SetRagdollState(args.isOn);
				PauseMovement(args.isOn, true);
			}
		}
        void OnEnable()
        {
			GlobalEventManager.Instance.Subscribe("CharacterReverseGravity", TempReverseGravity);
			GlobalEventManager.Instance.Subscribe<RagdollArgs>("Ragdoll", TempRagdoll);
        }
        void OnDisable()
        {
			if(GlobalEventManager.IsAvailable)
			{
				GlobalEventManager.Instance.Unsubscribe("CharacterReverseGravity", TempReverseGravity);
				GlobalEventManager.Instance.Unsubscribe<RagdollArgs>("Ragdoll", TempRagdoll);
			}            
        }

        protected virtual void FixedUpdate()
		{
			///TODO: 개선할 것 
			Controller.FloatValues.Find(x => x.name == "Horizontal").value = InputHandler.Horizontal;
			Controller.FloatValues.Find(x => x.name == "Vertical").value = InputHandler.Vertical;
		}
		protected virtual void Update()
		{
			///TODO: 개선할 것 
			Controller.FloatValues.Find(x => x.name == "Pitch").value = InputHandler.Pitch;
			Controller.FloatValues.Find(x => x.name == "Yaw").value = InputHandler.Yaw;

			for(int i = 0; i < InputHandler.ButtonValues.Count; ++i)
			{
				KeyValues<bool> value = Controller.BoolValues.Find(x => x.name == InputHandler.ButtonValues[i].name);
				if(value != null)
				{
					value.value = InputHandler.ButtonValues[i].value;
				}		
			}
			for(int i = 0; i < InputHandler.ToggleValues.Count; ++i)
			{
				KeyValues<bool> value = Controller.BoolValues.Find(x => x.name == InputHandler.ToggleValues[i].name);
				if(value != null)
				{
					value.value = InputHandler.ToggleValues[i].value;
				}		
			}
		}
		#endregion
		
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected virtual void Reset()
		{
			_isRespawning = false;
		}
		protected virtual void OnValidate()
		{
		}
		#endif
		#endregion
	}
}
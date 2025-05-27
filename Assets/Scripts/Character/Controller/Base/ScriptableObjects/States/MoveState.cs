///스크립트 생성 일자 - 2025 - 03 - 11
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using CHG.Utilities.Attribute;
using CHG.Utilities.Patterns;
using UnityEngine;

namespace TempNamespace.Character.Controller
{
	/// <summary>
	/// State Pattern 기반의 이동 시스템
	/// </summary>
	[Serializable]
	public abstract class MoveState : ScriptableObject
	{
		#region Inspector Fields
		[Header("State Identity")]
		[SerializeField, Tooltip("상태의 ID")]
		protected int _stateID;
		[SerializeField, Tooltip("상태 이름")]
		protected string _stateName;
		[SerializeField, Tooltip("이 상태의 종류"), ReadOnly(true, true)]
		protected MoveStateFlag _stateFlag;
		[SerializeField, Tooltip("이 상태에 대한 주석"), TextArea]
		protected string _annotation;

		[Header("State Status")]
		[SerializeField, Tooltip("속도 제한")]
		protected float _maxSpeed;
		[SerializeField, Tooltip("최대 가속도")]
		protected float _maxAcceleration;
		[SerializeField, Tooltip("최대 감속도")]
		protected float _maxDeceleration;
		
		
		#endregion

		#region Fields
		protected PhysicsBasedController _character;
		#endregion
		
		#region Properties
		/// <summary>
		/// 연동된 CharacterMovement
		/// </summary>
		public PhysicsBasedController Character
		{
			get => _character;
			set => _character = value;
		}
		/// <summary>
		/// 이 상태의 이름
		/// </summary>
		public string StateName
		{
			get => _stateName;
			set => _stateName = value;
		}
		/// <summary>
		/// 상태의 ID
		/// </summary>
		public int StateID
		{
		   get => _stateID;
		   set => _stateID = value;
		}
		/// <summary>
		/// 이 상태에 대한 주석
		/// </summary>
		public string Annotation
		{
		   get => _annotation;
		   set => _annotation = value;
		}
		
		/// <summary>
		/// 이 상태의 특징
		/// </summary>
		public MoveStateFlag StateFlag
		{
		   get => _stateFlag;
		   set => _stateFlag = value;
		}

		/// <summary>
		/// 이동 상태인가?
		/// </summary>
		public bool IsLocomotive => StateFlag.HasFlag(MoveStateFlag.Locomotive);
		
		/// <summary>
		/// 중력에 영향을 받는 상태인가?
		/// </summary>
		public bool UseGravity => !StateFlag.HasFlag(MoveStateFlag.DontUseGravity);
		
		/// <summary>
		/// 속도 제한
		/// </summary>
		public float MaxSpeed
		{
		   get => _maxSpeed;
		   set => _maxSpeed = Mathf.Max(0.0f, value);
		}
		/// <summary>
		/// 최대 가속도
		/// </summary>
		public float MaxAcceleration
		{
		   get => _maxAcceleration;
		   set => _maxAcceleration = Mathf.Max(0.0f, value);
		}
		/// <summary>
		/// 최대 감속도
		/// </summary>
		public float MaxDeceleration
		{
		   get => _maxDeceleration;
		   set => _maxDeceleration = Mathf.Max(0.0f, value);
		}
		#endregion

		#region Abstract Methods
		public virtual void OnActivated(PhysicsBasedController moveSystem)
		{

		}
		public virtual void OnDeactivated(PhysicsBasedController moveSystem)
		{

		}
		public virtual void OnStateEnter(PhysicsBasedController moveSystem, MoveStateFlag prevStateFlag)
		{

		}
		public virtual void OnStateExit(PhysicsBasedController moveSystem, MoveStateFlag nextStateFlag)
		{

		}

		public abstract void OnBeforeSimulate(PhysicsBasedController moveSystem);
		public abstract void OnSimulate(PhysicsBasedController moveSystem);
		public abstract void OnAfterSimulate(PhysicsBasedController moveSystem);
		public abstract void OnApplyMovement(PhysicsBasedController moveSystem);
		
		/// <summary>
		/// 이 상태로 진입할 수 있는 상태인지 체크
		/// </summary>
		public abstract bool CanTransitionHere(PhysicsBasedController moveSystem);		
		/// <summary>
		/// State 종료 여부 확인
		/// </summary>
		public abstract bool IsFinished(PhysicsBasedController moveSystem);
		#endregion

		#region Methods

		public virtual void Reset()
		{
		}
		/// <summary>
		/// 이 상태에서 다른 상태로 전환할 수 있는지 체크
		/// </summary>
		/// <param name="newState">새 상태</param>
		/// <returns>새 상태가 MoveState라면 true, 아니라면 false</returns>
        public virtual bool CanTransitionTo(IBaseState newState)
        {
			if(newState is MoveState)
				return true;
			else
				return false;
        }
		#endregion

		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		public virtual void OnValidate()
		{
			MaxSpeed = _maxSpeed;
			MaxAcceleration = _maxAcceleration;
			MaxDeceleration = _maxDeceleration;
		}
		#endif
		#endregion
    }
}
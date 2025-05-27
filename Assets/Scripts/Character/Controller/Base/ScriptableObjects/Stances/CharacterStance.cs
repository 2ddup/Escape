///스크립트 생성 일자 - 2025 - 03 - 19
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using System;
using UnityEngine;

namespace TempNamespace.Character.Controller
{	
	public class StanceWrapper : ScriptableObject
	{
		private CharacterStance _stance;
		
		public CharacterStance Stance
		{
			get => _stance;
			set => _stance = value;
		}
	}
	public abstract class CharacterStance : ScriptableObject
	{
		#region Inspector Fields
		[SerializeField, Tooltip("이 자세의 이름")]
		protected string _stanceName;		
		/// <summary>
		/// 이 자세의 이름
		/// </summary>
		public string StanceName
		{
		   get => _stanceName;
		   set => _stanceName = value;
		}
		[SerializeField, Tooltip("이 자세일 때 변경되는 Animator Parameter의 이름")]
		protected string _stanceParameterName = "StanceID";
		[SerializeField, Tooltip("이 자세일 때 변경되는 Animator Parameter의 값")]
		protected int _stanceParameterValue;		
		#endregion

		#region Fields
		protected bool _stanceActivated;
		protected int _stanceParameterHash;
		#endregion
		
		#region Properties		
		/// <summary>
		/// 이 자세일 때 변경되는 Animator Parameter의 값
		/// </summary>
		public int StanceParameterValue
		{
		   get => _stanceParameterValue;
		   set => _stanceParameterValue = value;
		}
		
		/// <summary>
		/// 이 자세일 때 토글되는 Animator ID의 이름
		/// </summary>
		public string StanceParameterName
		{
		   get => _stanceParameterName;
		   set
		   { 
				_stanceParameterName = value;
				_stanceParameterHash = Animator.StringToHash(_stanceParameterName);
		   }
		}
		
		/// <summary>
		/// 이 자세를 취하고 있는 상태인가?
		/// </summary>
		public bool StanceActivated
		{
			get => _stanceActivated;
			set => _stanceActivated = value;
		}
		#endregion

		#region Events		
		public virtual void ActivateStance(PhysicsBasedController moveSystem)
		{
			moveSystem.animator?.SetInteger(_stanceParameterHash, StanceParameterValue);
		}
		public virtual void DeactivateStance(PhysicsBasedController moveSystem)
		{
			moveSystem.animator?.SetInteger(_stanceParameterHash, 0);
		}
		#endregion

		#region Methods
		/// <summary>
		/// 자세 활성화 가능 여부
		/// </summary>
		public abstract bool CanActivateStance(PhysicsBasedController character);
		/// <summary>
		/// 자세 비활성화 가능 여부
		/// </summary>
		public abstract bool CanDeactivateStance(PhysicsBasedController character);

		public virtual void Reset()
		{
		}
		#endregion

		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			_stanceParameterHash = Animator.StringToHash(_stanceParameterName);
		}
		#endif
		#endregion
	}
}
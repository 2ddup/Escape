///스크립트 생성 일자 - 2025 - 03 - 20
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using UnityEngine;

namespace TempNamespace.Character.Controller
{
	public abstract class CharacterAction : ScriptableObject
	{
		#region Inspector Fields
		[Header("Action Identity")]
		[SerializeField, Tooltip("이 Action의 ID")]
		private int _actionID;
		[SerializeField, Tooltip("이 Action의 이름")]
		private string _actionName;
		
		
		#endregion

		#region Fields
		#endregion
		
		#region Properties
		/// <summary>
		/// 이 Action의 ID
		/// </summary>
		public int ActionID
		{
		   get => _actionID;
		   set => _actionID = value;
		}
		/// <summary>
		/// 이 Action의 이름
		/// </summary>
		public string ActionName
		{
		   get => _actionName;
		   set => _actionName = value;
		}
		#endregion

		#region Methods
		public abstract void OnBeforeSimulate(PhysicsBasedController moveSystem);
		public abstract void OnSimulate(PhysicsBasedController moveSystem);
		public abstract void OnAfterSimulate(PhysicsBasedController moveSystem);

		
		public virtual void Reset()
		{
		}
		#endregion

		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		public virtual void OnValidate()
		{
		}
		#endif
		#endregion
	}
}
///스크립트 생성 일자 - 2025 - 03 - 18
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using TempNamespace.Objects;
using UnityEngine;

namespace TempNamespace
{
	public class SceneChangeElevator : ElevatorObject
	{
		#region Inspector Fields
		
		#endregion

		#region Fields
		#endregion
		
		#region Properties
		#endregion

		#region	Events

		#endregion
		
		#region Methods
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected override void CacheComponents()
		{
			base.CacheComponents();
		}
		#endregion

		#region MonoBehaviour Methods
		protected override void Awake()
		{
			base.Awake();
		}
		#endregion
		
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected override void Reset()
		{
		}
		protected override void OnValidate()
		{
		}
		#endif
		#endregion
	}
}
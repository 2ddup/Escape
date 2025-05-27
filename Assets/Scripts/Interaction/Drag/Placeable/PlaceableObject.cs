///스크립트 생성 일자 - 2025 - 04 - 01
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;

namespace TempNamespace.ObjectDrag.Placeable
{
	public class PlaceableObject : GrabbableObject
	{
		#region Inspector Fields
		
		#endregion

		#region Fields
		#endregion
		
		#region Properties
		#endregion

		#region	Events

		#endregion
		
		#region MonoBehaviour Methods
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


        #region UnityEditor Only Methods
		#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
        }
		protected override void OnValidate()
		{
			base.OnValidate();
		}
		#endif
		#endregion
	}
}
///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;

namespace TempNamespace
{
	public class CubeSwitch : MonoBehaviour
	{
		#region Inspector Fields
		
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
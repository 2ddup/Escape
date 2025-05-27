///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;

namespace TempNamespace
{
	public class SwitchPuzzle : MonoBehaviour
	{
		#region Inspector Fields
		public string[] names;
		public Material[] materials;
		public GameObject[] cubes;
		public CubeSwitch[] switches;
		#endregion

		#region Fields

		#endregion
		
		#region Properties
		#endregion

		#region	Events

		#endregion
		
		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
        void Start()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// 컴퍼넌트를 캐싱
        /// </summary>	
        protected virtual void CacheComponents()
		{
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
///스크립트 생성 일자 - 2025 - 03 - 05
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using UnityEngine.UI;

namespace TempNamespace
{
	public class TempInteractRadar : MonoBehaviour
	{
		#region Inspector Fields
		public Image[] images;
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
		
		#region Methods
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			 _transform = GetComponent<Transform>();
		}
		public void Sync(Collider[] colliders, int counts)
		{
			Camera mainCam = Camera.main;
			for(int i = 0; i < images.Length; ++i)
			{
				if(counts > i)
				{
					images[i].gameObject.SetActive(true);
					Vector3 pos = mainCam.WorldToScreenPoint(colliders[i].transform.position);
					pos.x -= Screen.width/2;
					pos.y -= Screen.height/2;
					pos.z = 0;
					images[i].transform.localPosition = pos;
				}
				else
				{
					images[i].gameObject.SetActive(false);
				}
			}
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
		#endregion
	}
}
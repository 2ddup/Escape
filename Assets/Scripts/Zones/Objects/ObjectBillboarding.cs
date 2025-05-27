///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;

namespace TempNamespace
{
	public class ObjectBillboarding : MonoBehaviour
	{
		[SerializeField]
		Camera targetCam;

		void Awake()
		{
			CacheComponents();
		}
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			targetCam = Camera.main;
		}

        void LateUpdate()
        {
            if (targetCam != null)
			{
				transform.LookAt(targetCam.transform);
				// Y축을 기준으로만 회전하도록 설정합니다.
				Vector3 eulerAngles = transform.eulerAngles;
				eulerAngles.x = 0; // X축 회전 제거
				eulerAngles.z = 0; // Z축 회전 제거
				transform.eulerAngles = eulerAngles;
			}
        }
    }
}
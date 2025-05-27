///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using CHG.EventDriven;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TempNamespace
{
	public class TempPause : MonoBehaviour
	{
		public KeyCode pauseKey = KeyCode.Escape; // 일시정지 키
		public bool useTimescale = true; // 타임스케일 사용 여부

		public bool onPause;
		public GameObject PauseScreen;

		void Awake()
		{
			CacheComponents();
		}
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
		}

		private void Update() {
			if(Input.GetKeyDown(pauseKey))
			{
				onPause = !onPause;

				if(onPause)
				{
					GlobalEventManager.Instance.Publish("PauseCharacterControl");
					PauseScreen.SetActive(true);

					if(useTimescale)
					{
						Time.timeScale = 0;
					}
				}
				else
				{
					GlobalEventManager.Instance.Publish("ResumeCharacterControl");
					PauseScreen.SetActive(false);

					if(useTimescale)
					{
						Time.timeScale = 1;
					}
				}
			}
		}
		public void Resume()
		{
			if(useTimescale)
			{
				Time.timeScale = 1;
			}
			onPause = false;
			GlobalEventManager.Instance.Publish("ResumeCharacterControl");
			PauseScreen.SetActive(false);
		}
		public void ToTitleScene()
		{
			if(useTimescale)
			{
				Time.timeScale = 1;
			}
			SceneManager.LoadScene("Lobby 1");
		}

        void OnDestroy()
        {
			if(useTimescale)
			{
				if(Time.timeScale == 0)
				{
					Time.timeScale = 1;
				}
			}
        }
    }
}
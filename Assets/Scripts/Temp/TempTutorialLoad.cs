///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;
using UnityEngine.SceneManagement;


namespace TempNamespace
{
	public class TempTutorialLoad : MonoBehaviour
	{
		public void ToTutorial()
		{
			SceneManager.LoadScene("Level 1");
		}
	}
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LightSetting : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(AdjustLighting());
    }

    IEnumerator AdjustLighting()
    {
        yield return new WaitForEndOfFrame(); // 한 프레임 기다림

        // 환경 조명을 다시 적용 (씬별 설정에 맞게 수정)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; // 씬에 맞게 변경
        RenderSettings.ambientLight = new Color(122f / 255f, 139f / 255f, 159f / 255f);    // 필요한 경우 추가
    }
}
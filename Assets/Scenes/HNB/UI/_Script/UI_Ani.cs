using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_Ani : MonoBehaviour
{
    private Button start;  // 'start'라는 이름을 가진 버튼 참조
    private CanvasGroup canvasGroup;  // 버튼의 투명도 조절을 위한 CanvasGroup

    public float blinkDuration = 0.5f;  // 반짝임 효과 지속 시간
    public float blinkInterval = 0.1f;  // 반짝임 간격

    void Start()
    {
        // "start"라는 이름을 가진 버튼을 찾기
        start = GameObject.Find("start")?.GetComponent<Button>();  // 정확히 "start"라는 이름 사용

        // 버튼이 제대로 찾았는지 확인
        if (start == null)
        {
            Debug.LogError("start 버튼을 찾을 수 없습니다.");
            return;
        }

        // CanvasGroup이 버튼에 추가되어 있지 않다면 CanvasGroup 추가
        canvasGroup = start.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = start.gameObject.AddComponent<CanvasGroup>();
        }

        // 버튼 클릭 시 이벤트 등록
        start.onClick.AddListener(OnButtonClick);
    }

    // 버튼 클릭 시 호출되는 함수
    void OnButtonClick()
    {
        // 반짝임 효과 시작
        StartCoroutine(ApplyBlinkEffect());
    }

    // 반짝임 효과를 주는 코루틴
    IEnumerator ApplyBlinkEffect()
    {
        canvasGroup.alpha = 1f;

        for (int i = 0; i < blinkDuration / blinkInterval; i++)
        {
            // 반짝임 시작 (빠르게 투명도를 변경)
            canvasGroup.alpha = (canvasGroup.alpha == 1f) ? 0f : 1f;
            yield return new WaitForSeconds(blinkInterval);
        }

        // 반짝임이 끝난 후 버튼을 원래 상태로 설정 (완전히 보이게)
        canvasGroup.alpha = 1f;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_Ani : MonoBehaviour
{
    private Button start;  // 'start'��� �̸��� ���� ��ư ����
    private CanvasGroup canvasGroup;  // ��ư�� ���� ������ ���� CanvasGroup

    public float blinkDuration = 0.5f;  // ��¦�� ȿ�� ���� �ð�
    public float blinkInterval = 0.1f;  // ��¦�� ����

    void Start()
    {
        // "start"��� �̸��� ���� ��ư�� ã��
        start = GameObject.Find("start")?.GetComponent<Button>();  // ��Ȯ�� "start"��� �̸� ���

        // ��ư�� ����� ã�Ҵ��� Ȯ��
        if (start == null)
        {
            Debug.LogError("start ��ư�� ã�� �� �����ϴ�.");
            return;
        }

        // CanvasGroup�� ��ư�� �߰��Ǿ� ���� �ʴٸ� CanvasGroup �߰�
        canvasGroup = start.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = start.gameObject.AddComponent<CanvasGroup>();
        }

        // ��ư Ŭ�� �� �̺�Ʈ ���
        start.onClick.AddListener(OnButtonClick);
    }

    // ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    void OnButtonClick()
    {
        // ��¦�� ȿ�� ����
        StartCoroutine(ApplyBlinkEffect());
    }

    // ��¦�� ȿ���� �ִ� �ڷ�ƾ
    IEnumerator ApplyBlinkEffect()
    {
        canvasGroup.alpha = 1f;

        for (int i = 0; i < blinkDuration / blinkInterval; i++)
        {
            // ��¦�� ���� (������ ������ ����)
            canvasGroup.alpha = (canvasGroup.alpha == 1f) ? 0f : 1f;
            yield return new WaitForSeconds(blinkInterval);
        }

        // ��¦���� ���� �� ��ư�� ���� ���·� ���� (������ ���̰�)
        canvasGroup.alpha = 1f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // UI 요소들
    public GameObject start;
    public GameObject setting;
    public GameObject panel;
    public GameObject backButton;
    public GameObject exit;
    public GameObject save;

    // 각 패널 안의 UI 요소들 (빈 게임 오브젝트들)
    public GameObject musicPanelContent; // 음악 설정을 위한 UI 요소들
    public GameObject controlsPanelContent; // 컨트롤 설정을 위한 UI 요소들

    // Music, Controls 버튼
    public GameObject musicButton;
    public GameObject controlsButton;

    // Start는 처음 UI 설정
    void Start()
    {
        // Panel을 비활성화 상태로 설정 (처음에는 보이지 않도록 설정)
        if (panel != null)
        {
            panel.SetActive(false);  // 패널 비활성화
        }

        // 기본적으로 뮤직 패널의 콘텐츠만 활성화
        if (musicPanelContent != null)
        {
            musicPanelContent.SetActive(true);  // 음악 설정 UI 요소 활성화
        }

        // 다른 패널들의 콘텐츠는 비활성화
        if (controlsPanelContent != null)
        {
            controlsPanelContent.SetActive(false);
        }

        // 버튼은 계속 보이게 설정
        if (musicButton != null)
        {
            musicButton.SetActive(true);
        }

        if (controlsButton != null)
        {
            controlsButton.SetActive(true);
        }

        // Exit 버튼도 활성화
        if (exit != null)
        {
            exit.SetActive(true);
        }
    }

    // Setting 클릭 시 호출되는 함수
    public void OnSettingClick()
    {
        // Start, Setting, Exit 버튼을 비활성화
        if (start != null)
        {
            start.SetActive(false);  // Start 버튼 비활성화
        }

        if (setting != null)
        {
            setting.SetActive(false);  // Setting 버튼 비활성화
        }

        if (exit != null)
        {
            exit.SetActive(false);  // Exit 버튼 비활성화
        }

        if (save != null)
        {
            save.SetActive(false);  // save 버튼 비활성화
        }

        // Panel을 활성화
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    // Back 클릭 시 호출되는 함수
    public void OnBackClick()
    {
        // Start, Setting, Exit 버튼을 다시 활성화
        if (start != null)
        {
            start.SetActive(true);  // Start 버튼 다시 활성화
        }

        if (setting != null)
        {
            setting.SetActive(true);  // Setting 버튼 다시 활성화
        }

        if (exit != null)
        {
            exit.SetActive(true);  // Exit 버튼 다시 활성화
        }

        if (save != null)
        {
            save.SetActive(true);  // save 버튼 다시 활성화
        }

        // Panel을 비활성화
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Music 버튼 클릭 시 호출되는 함수
    public void ShowMusicPanel()
    {
        // 모든 패널 내용을 비활성화
        if (musicPanelContent != null)
        {
            musicPanelContent.SetActive(true);  // 음악 설정 UI 활성화
        }

        if (controlsPanelContent != null)
        {
            controlsPanelContent.SetActive(false); // 컨트롤 설정 UI 비활성화
        }
    }

    // Controls 버튼 클릭 시 호출되는 함수
    public void ShowControlsPanel()
    {
        // 모든 패널 내용을 비활성화
        if (musicPanelContent != null)
        {
            musicPanelContent.SetActive(false); // 음악 설정 UI 비활성화
        }

        if (controlsPanelContent != null)
        {
            controlsPanelContent.SetActive(true); // 컨트롤 설정 UI 활성화
        }
    }

    // Exit 버튼 클릭 시 호출되는 함수
    public void OnExitClick()
    {
        // 게임이 빌드에서 실행 중일 때 종료
#if UNITY_EDITOR
        // Unity 에디터에서 게임을 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 빌드된 게임에서 종료
            Application.Quit();
#endif
    }
}

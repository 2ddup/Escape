using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneSetting : MonoBehaviour
{
    public static GameSceneSetting Instance; // 싱글톤 인스턴스

    public GameObject settingPnl;
    public GameObject musicPanelContent;
    public GameObject controlsPanelContent;
    public GameObject musicButton;
    public GameObject controlsButton;
    
    private bool isPanelActive = false;

    void Awake()
    {
        // 싱글톤 패턴: 이미 인스턴스가 있다면 나 자신은 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복된 오브젝트는 파괴
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (settingPnl == null)
        {
            GameObject canvas = GameObject.FindGameObjectWithTag("SettingCanvas");
            settingPnl = canvas?.transform.Find("SettingPanel")?.gameObject;
        }

        if (settingPnl != null && settingPnl.activeSelf)
        {
            settingPnl.SetActive(false);
        }
        else
        {
            Debug.LogWarning("settingPnl이 할당되지 않았거나, null입니다.");
        }
    }

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


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (settingPnl != null && settingPnl.activeSelf)
        {
            settingPnl.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
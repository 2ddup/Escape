using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // UI ��ҵ�
    public GameObject start;
    public GameObject setting;
    public GameObject panel;
    public GameObject backButton;
    public GameObject exit;
    public GameObject save;

    // �� �г� ���� UI ��ҵ� (�� ���� ������Ʈ��)
    public GameObject musicPanelContent; // ���� ������ ���� UI ��ҵ�
    public GameObject controlsPanelContent; // ��Ʈ�� ������ ���� UI ��ҵ�

    // Music, Controls ��ư
    public GameObject musicButton;
    public GameObject controlsButton;

    // Start�� ó�� UI ����
    void Start()
    {
        // Panel�� ��Ȱ��ȭ ���·� ���� (ó������ ������ �ʵ��� ����)
        if (panel != null)
        {
            panel.SetActive(false);  // �г� ��Ȱ��ȭ
        }

        // �⺻������ ���� �г��� �������� Ȱ��ȭ
        if (musicPanelContent != null)
        {
            musicPanelContent.SetActive(true);  // ���� ���� UI ��� Ȱ��ȭ
        }

        // �ٸ� �гε��� �������� ��Ȱ��ȭ
        if (controlsPanelContent != null)
        {
            controlsPanelContent.SetActive(false);
        }

        // ��ư�� ��� ���̰� ����
        if (musicButton != null)
        {
            musicButton.SetActive(true);
        }

        if (controlsButton != null)
        {
            controlsButton.SetActive(true);
        }

        // Exit ��ư�� Ȱ��ȭ
        if (exit != null)
        {
            exit.SetActive(true);
        }
    }

    // Setting Ŭ�� �� ȣ��Ǵ� �Լ�
    public void OnSettingClick()
    {
        // Start, Setting, Exit ��ư�� ��Ȱ��ȭ
        if (start != null)
        {
            start.SetActive(false);  // Start ��ư ��Ȱ��ȭ
        }

        if (setting != null)
        {
            setting.SetActive(false);  // Setting ��ư ��Ȱ��ȭ
        }

        if (exit != null)
        {
            exit.SetActive(false);  // Exit ��ư ��Ȱ��ȭ
        }

        if (save != null)
        {
            save.SetActive(false);  // save ��ư ��Ȱ��ȭ
        }

        // Panel�� Ȱ��ȭ
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    // Back Ŭ�� �� ȣ��Ǵ� �Լ�
    public void OnBackClick()
    {
        // Start, Setting, Exit ��ư�� �ٽ� Ȱ��ȭ
        if (start != null)
        {
            start.SetActive(true);  // Start ��ư �ٽ� Ȱ��ȭ
        }

        if (setting != null)
        {
            setting.SetActive(true);  // Setting ��ư �ٽ� Ȱ��ȭ
        }

        if (exit != null)
        {
            exit.SetActive(true);  // Exit ��ư �ٽ� Ȱ��ȭ
        }

        if (save != null)
        {
            save.SetActive(true);  // save ��ư �ٽ� Ȱ��ȭ
        }

        // Panel�� ��Ȱ��ȭ
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Music ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void ShowMusicPanel()
    {
        // ��� �г� ������ ��Ȱ��ȭ
        if (musicPanelContent != null)
        {
            musicPanelContent.SetActive(true);  // ���� ���� UI Ȱ��ȭ
        }

        if (controlsPanelContent != null)
        {
            controlsPanelContent.SetActive(false); // ��Ʈ�� ���� UI ��Ȱ��ȭ
        }
    }

    // Controls ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void ShowControlsPanel()
    {
        // ��� �г� ������ ��Ȱ��ȭ
        if (musicPanelContent != null)
        {
            musicPanelContent.SetActive(false); // ���� ���� UI ��Ȱ��ȭ
        }

        if (controlsPanelContent != null)
        {
            controlsPanelContent.SetActive(true); // ��Ʈ�� ���� UI Ȱ��ȭ
        }
    }

    // Exit ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void OnExitClick()
    {
        // ������ ���忡�� ���� ���� �� ����
#if UNITY_EDITOR
        // Unity �����Ϳ��� ������ ����
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ����� ���ӿ��� ����
            Application.Quit();
#endif
    }
}

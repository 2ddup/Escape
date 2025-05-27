using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTargetSpawner : MonoBehaviour
{
    public GameObject trgtPrefab;
    public Vector3 startPosition = new Vector3(-2.0f, 4.6f, 10.0f); // 왼쪽 위 기준
    public float spacing = 1.5f; // 타겟 간격
    public int rows = 3;
    public int cols = 3;

    private List<GameObject> trgts = new List<GameObject>();
    private List<VRTarget> targetScripts = new List<VRTarget>();
    private bool gameStarted = false;
    private bool gameCleared = false;

    public void StartGame()
    {
        if (!gameStarted && !gameCleared)
        {
            gameStarted = true;
            SpawnTargets();
        }
    }

    public void Stop()
    {
        gameStarted = false;
        foreach (GameObject target in trgts)
        {
            if (target != null) Destroy(target);
        }

        trgts.Clear();
        targetScripts.Clear();

        Debug.Log("타겟 생성을 멈췄습니다.");
    }

    public void SpawnTargets()
    {
        if (gameCleared || !gameStarted) return;

        ClearOldTargets();
        targetScripts.Clear();
        trgts.Clear();

        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 pos = startPosition + new Vector3(col * spacing, -row * spacing, 0);
                Quaternion rot = Quaternion.Euler(90f, 0f, 0f); // 실린더 기준 회전
                //Quaternion rot_Head = Quaternion.Euler(0f, 0f, 0f); // 실린더 기준 회전
                GameObject target = Instantiate(trgtPrefab, pos, rot);
                trgts.Add(target);

                VRTarget targetScript = target.GetComponent<VRTarget>();
                if (targetScript != null)
                {
                    VRTarget.TargetColor color = VRTarget.GetRandomTargetColor();
                    targetScript.Setup(color, index);
                    targetScripts.Add(targetScript);
                    index++;
                }
            }
        }

        if (targetScripts.Count > 0)
        {
            VRTargetSequenceManager.Instance?.RegisterTargets(targetScripts);
        }
    }

    private void ClearOldTargets()
    {
        foreach (GameObject target in trgts)
        {
            if (target != null) Destroy(target);
        }
        trgts.Clear();
    }

    public void RemoveTargetFromList(GameObject target)
    {
        if (trgts.Contains(target))
        {
            trgts.Remove(target);

            VRTarget targetScript = target.GetComponent<VRTarget>();
            if (targetScript != null && VRTargetSequenceManager.Instance != null)
            {
                VRTargetSequenceManager.Instance.OnTargetDestroyed(targetScript);
            }

            // 모든 타겟 제거되면 재생성
            if (trgts.Count == 0 && gameStarted && !gameCleared)
            {
                SpawnTargets();
            }
        }
    }

    public void SetGameCleared()
    {
        gameCleared = true;
        Debug.Log("🎉 게임 클리어!");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

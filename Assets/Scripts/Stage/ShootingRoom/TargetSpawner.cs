using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject trgtPrefab;

    public Vector3 startPosition = new Vector3(-2.0f, 4.6f, 10.0f); // 왼쪽 위 기준
    public float spacing = 1.5f; // 타겟 간격
    public int rows = 2;
    public int cols = 2;

    private List<GameObject> trgts = new List<GameObject>();
    private List<Target> targetScripts = new List<Target>();
    private bool gameStarted = false;
    private bool gameCleared = false;
    private bool isGenerating = false;
    protected int respawnCounter = 0;
    public int netTargetColor;
    public List<int> targetViewIDs = new List<int>();
    PhotonView photonView;
    TargetSequenceManager targetSequenceManager;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        targetSequenceManager = GetComponent<TargetSequenceManager>();
    }
    void Start()
    {
        
    }

    public void StartGame()
    {
        if (!gameStarted && !gameCleared)
        {
            gameStarted = true;
            photonView.RPC("RequestStartGame", PhotonTargets.MasterClient);
        }
    }

    
    [PunRPC]
    void SpawnRPC()
    {
        if (isGenerating || !gameStarted || gameCleared) return;

        isGenerating = true;
        SpawnTargets();
    }

    
    public void SpawnTargets()
    {        
        if(PhotonNetwork.isMasterClient)
        {
            if (gameCleared || !gameStarted) 
            {
                isGenerating = false;
                return;
            }

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

                    GameObject target = PhotonNetwork.Instantiate("Target", pos, rot, 0);
                    trgts.Add(target);

                    Target targetScript = target.GetComponent<Target>();
                    if (targetScript != null)
                    {
                        int targetColor = Target.GetRandomTargetColor();
                        targetScripts.Add(targetScript);   //이후 RPC에서 target의 포톤뷰 아이디 가져와서 넣음
                        targetScript.Setup(targetColor, index);

                        index++;
                    }
                }
            }

            if (targetScripts.Count > 0)
            {
                foreach(Target targetscript in targetScripts)
                {
                    targetViewIDs.Add(targetscript.GetComponent<PhotonView>().viewID);
                }

                TargetSequenceManager.Instance?.RegisterTargets(targetScripts);

                photonView.RPC("ReceiveTargetList", PhotonTargets.Others, targetViewIDs.ToArray());
                targetViewIDs.Clear();
            }
        }
        
        isGenerating = false;
    }

    [PunRPC]
    public void ReceiveTargetList(int[] targetIDs)
    {
        targetViewIDs = new List<int>(targetIDs);
        StartCoroutine(BuildTargetsWhenReady());
    }

    private IEnumerator BuildTargetsWhenReady()
    {
        float timeout = 2f;
        float timer = 0f;

        while (!AllTargetsReady() && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        trgts.Clear();
        targetScripts.Clear();
        targetSequenceManager.targetSequence.Clear();

        foreach (int viewID in targetViewIDs)
        {
            PhotonView view = PhotonView.Find(viewID);
            if (view != null)
            {
                GameObject go = view.gameObject;
                trgts.Add(go);

                Target target = go.GetComponent<Target>();
                if (target != null)
                {
                    targetScripts.Add(target);
                    targetSequenceManager.targetSequence.Add(target);
                }
            }
            else
            {
                Debug.LogWarning($"PhotonView {viewID} 못 찾음 (null)");
            }
        }

        Debug.Log("클라이언트에서 trgts / targetSequence 구성 완료");
    }

    private bool AllTargetsReady()
    {
        foreach (int viewID in targetViewIDs)
        {
            if (PhotonView.Find(viewID) == null)
                return false;
        }
        return true;
    }
    
    public void SortTrgtsAndScriptsByViewIDOrder()
    {
        List<GameObject> sortedTrgts = new List<GameObject>();
        List<Target> sortedScripts = new List<Target>();

        foreach (int id in targetViewIDs)
        {
            GameObject go = PhotonView.Find(id)?.gameObject;
            if (go != null)
            {
                sortedTrgts.Add(go);
                sortedScripts.Add(go.GetComponent<Target>());
            }
        }

        trgts = sortedTrgts;
        targetScripts = sortedScripts;
    }

    private void ClearOldTargets()
    {
        foreach (GameObject target in trgts)
        {
            if (target != null) Destroy(target);
        }
        trgts.Clear();
    }
    public void CheckRespawn()
    {
        if (PhotonNetwork.isMasterClient && gameStarted && !gameCleared)
        {
            if(respawnCounter == PhotonNetwork.playerList.Length)
            {
                photonView.RPC("RequestStartGame", PhotonTargets.MasterClient);
                respawnCounter = 0;
            }
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

    [PunRPC]
    void AddClearCount()
    {
        respawnCounter += 1;
        
        if (respawnCounter == PhotonNetwork.playerList.Length)
        { 
            CheckRespawn();
        }
    }

    public void RemoveTargetFromList(GameObject target)
    {
        if (trgts.Contains(target))
        {
            trgts.Remove(target);

            Target targetScript = target.GetComponent<Target>();
            if (targetScript != null && TargetSequenceManager.Instance != null)
            {
                TargetSequenceManager.Instance.OnTargetDestroyed(targetScript);
            }

            if(trgts.Count == 0)
            {
                photonView.RPC("AddClearCount", PhotonTargets.MasterClient);
            }
        }
    }

    public void SetGameCleared()
    {
        gameCleared = true;
        Debug.Log("🎉 게임 클리어!");
    }

    [PunRPC]
    void RequestStartGame()
    {
        // ❌ 이 조건은 RPC 자체가 마스터한테만 왔으니 불필요
        // if (!PhotonNetwork.isMasterClient) return;

        if (isGenerating || gameCleared) return;  // ✅ 이 조건만 남기자
        if (!gameStarted) gameStarted = true;

        photonView.RPC("SpawnRPC", PhotonTargets.AllBuffered);
    }
    
}

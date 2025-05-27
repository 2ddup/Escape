using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRScoreManager : MonoBehaviour
{
    public static VRScoreManager Instance{get; private set;}
    private int score = 0;
    public GameObject scorePopupPrefab;
    public Text totalScoreTxt;
    public TextMeshPro gameClearTxt;
    public GameObject[] pangEffectPrefabs; // 파티클 효과 프리팹
    public Transform[] particlePositions;
    public GameObject targetSpawner; // 타겟 생성 관리 객체
    private bool gameCleared = false; // 게임 클리어 상태
    public int clearSCore = 0;

    private void Awake()
    {
        if(Instance == null)
        {
           Instance = this;
        }
        else
           Destroy(gameObject);
    }

    private void UpdateScoreUI()
    {
        if(totalScoreTxt != null)
        {
            totalScoreTxt.text = "Score : " + score.ToString();
        }
    }

    public void AddScore(int value, Vector3 position)
    {
        if(gameCleared) return; // 게임 클리어 시 점수 추가를 막음
        
        score += value;
        UpdateScoreUI(); //점수판 갱신!

        ShowScorePopup(value, position);

        if (score >= clearSCore && !gameCleared) // 점수가 10점 이상이면 게임 클리어
        {
            gameCleared = true;
            HandleGameClear();
            SoundManager.Instance.PlaySFX("ShootClear", false); // 게임 클리어 사운드 재생
        }
    }

    private void ShowScorePopup(int value, Vector3 position)
    {        
        if(scorePopupPrefab != null)
        {
            // 팝업 생성
            GameObject popup = Instantiate(scorePopupPrefab, position, Quaternion.identity);
            
            // 점수 값 설정
            ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
            if (scorePopup != null)
            {
                scorePopup.Setup(value);
            }

            Destroy(popup, 10.0f);
        }
    }

    private void HandleGameClear()
    {
        // 타겟 생성을 멈추는 로직
        if (targetSpawner != null)
        {
            // 타겟 생성 스크립트에서 멈추는 함수 호출
            targetSpawner.GetComponent<VRTargetSpawner>().Stop();
        }

        StartCoroutine(ShowClearSequence());
    }

        private IEnumerator ShowClearSequence()
    {
        if (totalScoreTxt != null)
            totalScoreTxt.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        if (gameClearTxt != null)
            gameClearTxt.gameObject.SetActive(true);

        if (pangEffectPrefabs != null)
        {
            for(int i = 0; i < pangEffectPrefabs.Length; i++)
            {
                GameObject pang = Instantiate(pangEffectPrefabs[i], particlePositions[i].position, particlePositions[i].localRotation);
                Destroy(pang, 3.0f);
            }
        }
    }

    public int GetScore()
    {
        return score;
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

using UnityEngine;
using TMPro;
using System.Collections;
using TempNamespace.InteractableObjects;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance{get; private set;}
    private int score = 0;
    public int clearScore = 10;
    public GameObject scorePopupPrefab;
    public Text totalScoreTxt;
    public GameObject[] pangEffectPrefabs; // 파티클 효과 프리팹
    public Transform[] particlePositions;

    public GameObject targetSpawner; // 타겟 생성 관리 객체

    public Transform elevatorButt; //엘레베이터 활성화
    public Renderer lightRenderer; // Light 오브젝트의 Renderer (MeshRenderer 등)
    private bool gameCleared = false; // 게임 클리어 상태
    GameObject[] walls;
    public UnityEvent ONScoreReached;

    private void Awake()
    {
        if(Instance == null)
        {
           Instance = this;
        }
        else
           Destroy(gameObject);

        walls = GameObject.FindGameObjectsWithTag("Wall");
    }
    void Start()
    {
        
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

        if (score >= clearScore && !gameCleared) // 점수가 clearScore 이상이면 게임 클리어
        {
            gameCleared = true;
            HandleGameClear();
            
            for(int i = 0 ; i < walls.Length ; i++)
            {
                walls[i].SetActive(false);
            }

            SoundManager.Instance.PlaySFX("ShootClear", false); // 게임 클리어 사운드 재생
            ONScoreReached?.Invoke();
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

            Destroy(popup, 2.0f);
        }
    }

    private void HandleGameClear()
    {
        // 타겟 생성을 멈추는 로직
        if (targetSpawner != null)
        {
            // 타겟 생성 스크립트에서 멈추는 함수 호출
            targetSpawner.GetComponent<TargetSpawner>().Stop();
        }

        StartCoroutine(ShowClearSequence());
    }

    private IEnumerator ShowClearSequence()
    {
        if (totalScoreTxt != null)
        {
            totalScoreTxt.text = "Clear!";

            for(int i = 0; i < pangEffectPrefabs.Length; i++)
            {
                GameObject pang = Instantiate(pangEffectPrefabs[i], particlePositions[i].position, particlePositions[i].localRotation);
                Destroy(pang, 3.0f);
            }
        }
        yield return new WaitForSeconds(1.0f);

        if(elevatorButt != null)
        {
            SimpleInteractable interactable = elevatorButt.GetComponent<SimpleInteractable>();
            if (interactable != null)
            {
                interactable.IsInteractable = true;
            }

            if (lightRenderer != null)
            {
                lightRenderer.material.color = Color.green;
            }
        }
    }

    public int GetScore()
    {
        return score;
    }
}
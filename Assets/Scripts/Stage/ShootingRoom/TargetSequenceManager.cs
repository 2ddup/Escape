using System.Collections.Generic;
using UnityEngine;

public class TargetSequenceManager : MonoBehaviour
{
    public static TargetSequenceManager Instance;

    public List<Target> targetSequence = new List<Target>();
    public int currentIndex = 0;
    List<int> colors =  new List<int>();
    PhotonView photonView;
    TargetSpawner targetSpawner;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        photonView = GetComponent<PhotonView>();
        targetSpawner = GetComponent<TargetSpawner>();
    }
    void Start()
    {
        
    }

    // 타겟들을 한 번에 등록하고 순서 섞기
    public void RegisterTargets(List<Target> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("등록할 타겟이 없습니다.");
            return;
        }

        targetSequence = new List<Target>(targets);
        Shuffle(targetSequence);
        
        foreach(Target target in targetSequence)
        {
            colors.Add((int)target.trgtColor); 
        }
        
        for (int i = 0; i < targetSequence.Count; i++)
        {
            // 1번부터 시작하는 순서 지정
            targetSequence[i].SetOrderNumber(i + 1);
            // photonView.RPC("SyncTargetNumber",PhotonTargets.Others, i, i+1);
            // photonView.RPC("SyncTargetColor",PhotonTargets.Others, i, colors[i]);

        }

        currentIndex = 0;
        photonView.RPC("SyncCurrentIndex", PhotonTargets.Others, 0);

        Debug.Log("타겟 순서 등록 완료");
    }

    
    // 현재 타겟이 맞춰야 할 순서인지 확인
    public bool IsCorrectTarget(Target target, BulletColor bulletColor)
    {
        if (target == null || currentIndex >= targetSequence.Count)
            return false;

        Target currentTarget = targetSequence[currentIndex];

        if (target != currentTarget)
            return false;

        // 보라색은 양쪽 총알이 맞았는지에 따라 결정됨
        if (target.trgtColor == Target.TargetColor.Purple)
        {
            return target.IsFullyHit();
        }

        // 빨강/파랑은 맞춘 총알 색이 정확한지만 확인
        return (bulletColor == BulletColor.Red && target.trgtColor == Target.TargetColor.Red) ||
               (bulletColor == BulletColor.Blue && target.trgtColor == Target.TargetColor.Blue);
    }

    // 절반만 맞았을 때 호출됨 (보라색 전용)
    public void MarkHalfHit(Target target, BulletColor bulletColor)
    {
        if (target == null || currentIndex >= targetSequence.Count) return;

        if (targetSequence[currentIndex] == target)
        {
            // 총알 색이 올바른 경우에만 절반 히트로 인정
            if ((bulletColor == BulletColor.Red && target.trgtColor == Target.TargetColor.Purple) ||
                (bulletColor == BulletColor.Blue && target.trgtColor == Target.TargetColor.Purple))
            {
                Debug.Log($"✅ [절반 히트] 순서대로 맞춘 보라색 타겟의 일부가 맞았습니다.)");
            }
        }
    }

    // 타겟이 성공적으로 파괴되었을 때 호출됨
    public void OnTargetDestroyed(Target target)
    {
        if (target == null) return;

        if (currentIndex < targetSequence.Count && targetSequence[currentIndex] == target)
        {
            currentIndex++;
            Debug.Log($"🎯 정답 타겟 파괴됨. 다음 순서: {currentIndex + 1}");
        }
    }

    // 리스트 섞기
    private void Shuffle(List<Target> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }

        for(int i = 0 ; i < list.Count ; i++)
        {
            for(int j =0 ; j < targetSpawner.targetViewIDs.Count ; j++)
            {
                if(list[i].GetComponent<PhotonView>().viewID == targetSpawner.targetViewIDs[j])
                {
                    int temp = targetSpawner.targetViewIDs[i];
                    targetSpawner.targetViewIDs[i] = targetSpawner.targetViewIDs[j];
                    targetSpawner.targetViewIDs[j] = temp;
                }
            }
        }
        photonView.RPC("ReceiveTargetSortedList", PhotonTargets.Others, targetSpawner.targetViewIDs.ToArray());
        targetSpawner.SortTrgtsAndScriptsByViewIDOrder();
    }

    [PunRPC]
    public void SyncCurrentIndex(int index)
    {
        if(index ==0)
        {
            currentIndex = 0;
        }
        else
        {
            currentIndex += index;
        }
    }

    [PunRPC]
    public void ReceiveTargetSortedList(int[] targetIDs)
    {
        TargetSpawner targetSpawner = GetComponent<TargetSpawner>();
        targetSpawner.targetViewIDs.AddRange(targetIDs);
    }
}





    // [PunRPC]
    // public void SyncTargetNumber(int index, int num)
    // {
    //     targetSequence[index].orderNumber = num;
    // }

    // [PunRPC]
    // public void SyncTargetColor(int index, int targetColor)
    // {
    //     targetSequence[index].trgtColor = (Target.TargetColor)targetColor;
    // }
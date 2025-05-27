using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTargetSequenceManager : MonoBehaviour
{
    public static VRTargetSequenceManager Instance;
    private List<VRTarget> targetSequence = new List<VRTarget>();
    private int currentIndex = 0;
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
    }
    public void RegisterTargets(List<VRTarget> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("등록할 타겟이 없습니다.");
            return;
        }

        targetSequence = new List<VRTarget>(targets);
        Shuffle(targetSequence);

        for (int i = 0; i < targetSequence.Count; i++)
        {
            // 1번부터 시작하는 순서 지정
            targetSequence[i].SetOrderNumber(i + 1);
        }

        currentIndex = 0;
        Debug.Log("타겟 순서 등록 완료");
    }

    public bool IsCorrectTarget(VRTarget target, BulletColor bulletColor)
    {
        if (target == null || currentIndex >= targetSequence.Count)
            return false;

        VRTarget currentTarget = targetSequence[currentIndex];

        if (target != currentTarget)
            return false;

        // 보라색은 양쪽 총알이 맞았는지에 따라 결정됨
        if (target.trgtColor == VRTarget.TargetColor.Purple)
        {
            return target.IsFullyHit();
        }

        // 빨강/파랑은 맞춘 총알 색이 정확한지만 확인
        return (bulletColor == BulletColor.Red && target.trgtColor == VRTarget.TargetColor.Red) ||
               (bulletColor == BulletColor.Blue && target.trgtColor == VRTarget.TargetColor.Blue);
    }

    public void MarkHalfHit(VRTarget target, BulletColor bulletColor)
    {
        if (target == null || currentIndex >= targetSequence.Count) return;

        if (targetSequence[currentIndex] == target)
        {
            // 총알 색이 올바른 경우에만 절반 히트로 인정
            if ((bulletColor == BulletColor.Red && target.trgtColor == VRTarget.TargetColor.Purple) ||
                (bulletColor == BulletColor.Blue && target.trgtColor == VRTarget.TargetColor.Purple))
            {
                Debug.Log($"✅ [절반 히트] 순서대로 맞춘 보라색 타겟의 일부가 맞았습니다.)");
            }
        }
    }

    public void OnTargetDestroyed(VRTarget target)
    {
        if (target == null) return;

        if (currentIndex < targetSequence.Count && targetSequence[currentIndex] == target)
        {
            currentIndex++;
            Debug.Log($"🎯 정답 타겟 파괴됨. 다음 순서: {currentIndex + 1}");
        }
    }

    private void Shuffle(List<VRTarget> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}

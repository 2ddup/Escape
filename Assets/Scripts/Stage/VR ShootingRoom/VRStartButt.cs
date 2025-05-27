using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRStartButt : MonoBehaviour
{
    private VRTargetSpawner trgtSpawner;
    private Collider buttColl;
    // Start is called before the first frame update
    void Start()
    {
        trgtSpawner = FindObjectOfType<VRTargetSpawner>();
        buttColl = GetComponent<Collider>();
    }

    public void OnMouseDown()
    {
        SoundManager.Instance.PlaySFX("ShootStart",false);
        if(trgtSpawner != null)
        {
            trgtSpawner.StartGame(); // 과녁 생성
            if (buttColl != null)
            {
                buttColl.enabled = false; // 버튼 클릭 비활성화
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EletronicLaser : MonoBehaviour
{
    public LineRenderer lineRender;
    public Transform laserStartPoint;
    public float laserRange = 3f;
    public LayerMask hitLayers;
    private float startXPos;

    public Transform playerRespawn;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        startXPos = laserStartPoint.position.x;  // 처음 배치된 X 위치 저장
    }

    void Update()
    {
        MoveLaser();
        ShootLaser();
    }

    void MoveLaser()
    {
        float offset = Mathf.Sin(Time.time * 2f) * 3f; // 좌우로 움직이기
        laserStartPoint.position = new Vector3(startXPos + offset, laserStartPoint.position.y, laserStartPoint.position.z);
    }

    void ShootLaser()
    {
        lineRender.SetPosition(0, laserStartPoint.position);

        RaycastHit hit;

        if (Physics.Raycast(laserStartPoint.position, laserStartPoint.forward, out hit, laserRange, hitLayers))
        {
            if(hit.collider.CompareTag("Barrier"))
            {
                Debug.Log("베리어가 막음!");
                lineRender.SetPosition(1, hit.point);
                return;
            }

            lineRender.SetPosition(1, hit.point);
            CheckPlayerHit(hit.collider);
        }
        else
        {
            lineRender.SetPosition(1, laserStartPoint.position + laserStartPoint.forward * laserRange);
        }
    }

    void CheckPlayerHit(Collider coll)
    {
        if(coll.CompareTag("Player"))
        {
            Debug.Log("플레이어가 레이저에 맞음!");
            coll.gameObject.SetActive(false);
            Invoke("RespawnPlayer", 2f);
        }
    }

    void RespawnPlayer()
    {
        if (player != null && playerRespawn != null)
        {
            player.transform.position = playerRespawn.position; // 리스폰 위치로 이동
            player.SetActive(true); // 플레이어 활성화
            Debug.Log("활성화 됨!");
        }
        else
        {
            Debug.LogWarning("리스폰 위치가 설정되지 않았거나, 플레이어를 찾을 수 없습니다.");
        }
    }
}
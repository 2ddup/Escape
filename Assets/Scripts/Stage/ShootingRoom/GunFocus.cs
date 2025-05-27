using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunFocus : MonoBehaviour
{
    public Transform muzzleTransform;
    public RectTransform gunFocusImg;
    public float maxDistance = 30f;
    public Camera playerCam;

    // Update is called once per frame
    void Update()
    {
        // 1. 총구에서 forward 방향으로 일정 거리만큼 날아간 지점을 구함
    Vector3 predictedHitPoint;

    Ray ray = new Ray(muzzleTransform.position, muzzleTransform.forward);
    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
    {
        predictedHitPoint = hit.point;
    }
    else
    {
        predictedHitPoint = muzzleTransform.position + muzzleTransform.forward * maxDistance;
    }

    // 2. 그 위치를 카메라 기준 화면 위치로 변환
    Vector3 screenPos = playerCam.WorldToScreenPoint(predictedHitPoint);
    
    // 3. 조준점 위치를 해당 스크린 위치에 고정
    gunFocusImg.transform.position = screenPos;
    }
}

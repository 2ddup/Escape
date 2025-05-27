using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using TempNamespace;
using UnityEngine;

public class StopCameraHolder : MonoBehaviour, ICameraHold
{   
    Camera cam;
    CameraController cameraController;
    StopPuzzle stopPuzzle;
    PhotonView photonView;
    public GameObject focus;
    public void ChangeCameraHolder()
    {
       cameraController.TargetTransform = transform;
       GlobalEventManager.Instance.Publish("PauseCharacterControl");
    }

    void Awake()
    {
        stopPuzzle = transform.root.GetComponent<StopPuzzle>();
        photonView = stopPuzzle.GetComponent<PhotonView>();
    }

    IEnumerator Start()
    {
        while (GameObject.FindGameObjectsWithTag("Player").Length < 2)
            yield return null;

        cameraController = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<CameraController>();
        cam = Camera.main;
    }

    void Update()
    {
        if(!stopPuzzle.isStart && !stopPuzzle.isSolved)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit))
                {
                    if(hit.collider.CompareTag("StopStartButton"))
                    {   
                        stopPuzzle.isStart = true;
                        ChangeCameraHolder();
                        int myPlayerID = PhotonNetwork.player.ID; 
                        photonView.RPC("SyncIsStart", PhotonTargets.All, myPlayerID);
                        focus.SetActive(false);
                    }
                }
            }
        }
    } 
}

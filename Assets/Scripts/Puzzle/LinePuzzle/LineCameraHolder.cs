using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using TempNamespace;
using UnityEngine;

public class LineCameraHolder : MonoBehaviour, ICameraHold
{   
    Camera cam;
    CameraController cameraController;
    LinePuzzle linePuzzle;
    PhotonView photonView;
    public GameObject focus;
    public void ChangeCameraHolder()
    {
        cameraController.TargetTransform = transform;
        GlobalEventManager.Instance.Publish("PauseCharacterControl");
    }

    void Awake()
    {
        linePuzzle = GameObject.FindGameObjectWithTag("LinePuzzle").GetComponent<LinePuzzle>();
        photonView = linePuzzle.GetComponent<PhotonView>();
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
        if(!linePuzzle.isStart && !linePuzzle.isSolved)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit))
                {
                    if(hit.collider.CompareTag("LineStartButton"))
                    {   
                        linePuzzle.isStart = true;
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

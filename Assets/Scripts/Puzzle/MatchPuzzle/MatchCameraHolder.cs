using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using TempNamespace;
using UnityEngine;

public class MatchCameraHolder : MonoBehaviour, ICameraHold
{   
    Camera cam;
    CameraController cameraController;
    MatchPuzzle matchPuzzle;
    PhotonView photonView;
    public GameObject focus;
    public void ChangeCameraHolder()
    {
       cameraController.TargetTransform = transform;
       GlobalEventManager.Instance.Publish("PauseCharacterControl");
    }

    void Awake()
    {
        matchPuzzle = transform.root.GetComponent<MatchPuzzle>();
        photonView = matchPuzzle.GetComponent<PhotonView>();
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
        if(!matchPuzzle.isStart && !matchPuzzle.isSolved)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit))
                {
                    if(hit.collider.CompareTag("MatchStartButton"))
                    {   
                        matchPuzzle.isStart = true;
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

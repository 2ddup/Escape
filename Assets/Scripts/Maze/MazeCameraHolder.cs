using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using TempNamespace;
using UnityEngine;

public class MazeCameraHolder : MonoBehaviour
{
    Camera cam;
    CameraController cameraController;
    Controller controller;
    PhotonView photonView;
    public GameObject focus;
    public GameObject mazeExplain;
    public int hiddenClueCount;
    
    public void ChangeCameraHolder()
    {
       cameraController.TargetTransform = transform;
       GlobalEventManager.Instance.Publish("PauseCharacterControl");
    }

    void Awake()
    {
        controller = transform.root.GetComponentInChildren<Controller>();
        photonView = controller.GetComponent<PhotonView>();
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
        if(!controller.isStart && !controller.isSolved)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit))
                {
                    if(hit.collider.CompareTag("MazeStartButton") && hiddenClueCount == 3)
                    {
                        controller.isStart = true;
                        ChangeCameraHolder();
                        int myPlayerID = PhotonNetwork.player.ID;
                        photonView.RPC("SyncIsStart", PhotonTargets.All, myPlayerID);
                        focus.SetActive(false);
                        mazeExplain.SetActive(true);
                        controller.isPopupActive = true;
                    }
                }
            }
        }
    }
}

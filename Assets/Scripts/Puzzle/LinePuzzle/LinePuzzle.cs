using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TempNamespace;
using CHG.EventDriven;
using TempNamespace.InteractableObjects;

public class LinePuzzle : MonoBehaviour
{
    LineRenderer linerenderer;
    List<Vector3> linePoint = new List<Vector3>();
    Camera cam;
    public GameObject plane;
    public float minDistance = 0.05f;
    public Transform startPoint;
    public Transform endPoint;
    public float linePosZ = 6.25f;
    public GameObject LinePuzzleDesk;
    [HideInInspector] public bool isStart = false;
    CameraController cameraController;
    GameObject player;
    public bool isPuzzleIsInteracting = false;
    public bool isSolved = false;
    PhotonView photonView;
    public GameObject focus;
    public SimpleInteractable hiddenDoor;
    private bool isDrawingStart = false;
    // Start is called before the first frame update
    void Awake()
    {
        linerenderer = GetComponentInChildren<LineRenderer>();
        photonView = GetComponent<PhotonView>();
        //cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        //cam = Camera.main;
        //player = GameObject.FindGameObjectWithTag("Player");
    }

    IEnumerator Start()
    {
        while (GameObject.FindGameObjectWithTag("MainCamera") == null)
            yield return null;

        //cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam = Camera.main;

        linerenderer.positionCount = 0;
        linerenderer.startColor = Color.black;
        linerenderer.endColor = Color.black;
        linerenderer.startWidth = 0.03f;
        linerenderer.endWidth = 0.03f;
                                            //Debug.Log(cam.WorldToScreenPoint(plane.transform.position).z);
                                            //Debug.Log(startPoint.transform.position);
                                            //Debug.Log(endPoint.transform.position);
    }

    // Update is called once per frame
    void Update()
    {   
        if(isDrawingStart == true)
        {
            SoundManager.Instance.PlaySFX("MazeDrawLine", true);
            isDrawingStart = false;
        }

        if (!isStart) return;

        if(isPuzzleIsInteracting)
        {
            if(Input.GetMouseButtonDown(0))
            {
                isDrawingStart = true;
                linePoint.Clear();
                linerenderer.positionCount = 0;
                photonView.RPC("LineSetZero", PhotonTargets.Others);
            }

            if(Input.GetMouseButton(0))
            {
                Vector3 mousePos = Input.mousePosition;                         //일반적으로 Input.mousePosition를 하면 (x,y,0)으로 리턴함
                mousePos.z = linePosZ;                                          // z가 0이기 때문에 worldtoscreenpoint를 이용해 카메라에서 매개변수의 좌표까지의 거리를 z값으로 설정해줌
                Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);            //위에서 설정한 마우스의 좌표값을 월드좌표로 변환함 

                if(linePoint.Count == 0 || Vector3.Distance(linePoint[linePoint.Count-1], worldPos) > minDistance)
                {
                    if(linePoint.Count > 1)
                    {
                        Physics.Linecast(linePoint[linePoint.Count - 1], worldPos, out RaycastHit hitinfo);

                        if(hitinfo.transform != null && hitinfo.transform.CompareTag("LinePuzzleWall"))
                        {
                            linePoint.Clear();
                            linerenderer.positionCount = 0;
                            photonView.RPC("LineSetZero", PhotonTargets.Others);
                        }
                    }

                    linePoint.Add(worldPos);
                    linerenderer.positionCount = linePoint.Count;
                    linerenderer.SetPosition(linePoint.Count-1, worldPos);
                    photonView.RPC("SyncLine", PhotonTargets.Others, worldPos, linePoint.Count);
                }
            }

            if(Input.GetMouseButtonUp(0)) // 마우스 버튼에서 손을 떼면
            {
                SoundManager.Instance.StopLoopingSFX();
                isDrawingStart = false;
            }
        }

        if(linePoint.Count > 0 && isStart)
        {
            if(Vector3.Distance(startPoint.transform.position, linePoint[0]) < 0.3f && Vector3.Distance(endPoint.transform.position, linePoint[linePoint.Count-1]) < 0.1f )
            {
                Debug.Log("Solve Puzzle");
                LinePuzzleDesk.transform.DOLocalMoveZ(0.4f, 2f);
                GlobalEventManager.Instance.Publish("FocusToCharacter");
                GlobalEventManager.Instance.Publish("ResumeCharacterControl");
                isSolved = true;
                isStart = false;
                focus.SetActive(true);
                hiddenDoor.IsInteractable = true;
                SoundManager.Instance.PlaySFX("MazeDrawOpen", false);
                photonView.RPC("SyncClueInteractorble", PhotonTargets.Others);
                photonView.RPC("SyncIsSolved", PhotonTargets.Others);
                    
            }
        }


        if(Input.GetKeyDown(KeyCode.Escape))
        {
            photonView.RPC("SyncUnInteracting",PhotonTargets.All);
            GlobalEventManager.Instance.Publish("FocusToCharacter");
            GlobalEventManager.Instance.Publish("ResumeCharacterControl");
            focus.SetActive(true);
        }
    }

    [PunRPC]
    public void SyncIsStart(int ownerID)
    {
        isStart = true;
        isPuzzleIsInteracting = PhotonNetwork.player.ID == ownerID;
        if(isPuzzleIsInteracting)
        {
            if(!photonView.isMine)
			{
                PhotonView[] photonViews = GetComponentsInChildren<PhotonView>();
                foreach(PhotonView pv in photonViews)
                {
				    pv.TransferOwnership(PhotonNetwork.player);
                }
                LinePuzzleDesk.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player);
            }
        }
    }

    [PunRPC]
    public void SyncUnInteracting()
    {
        isStart = false;
        isPuzzleIsInteracting = false;
    }

    [PunRPC]
    public void SyncLine(Vector3 worldPos, int linePoint_Count)
    {
        linePoint.Add(worldPos);
        linerenderer.positionCount = linePoint_Count;
        linerenderer.SetPosition(linePoint.Count - 1, worldPos);
    }

    [PunRPC]
    public void LineSetZero()
    {
        linePoint.Clear();
        linerenderer.positionCount = 0;
    }

    [PunRPC]
    public void SyncIsSolved()
    {
        isSolved = true;
    }

    [PunRPC]
    public void SyncClueInteractorble()
    {
        hiddenDoor.IsInteractable = true; 
        
    }
}



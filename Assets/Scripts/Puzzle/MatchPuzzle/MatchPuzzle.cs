using System.Collections;
using System.Collections.Generic;
using TempNamespace;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using CHG.EventDriven;
using TempNamespace.InteractableObjects;

public class MatchPuzzle : MonoBehaviour
{
    public static MatchPuzzle instance;
    Camera cam;
    public GameObject selectedObject;
    public Vector3 originPos;
    RaycastHit hit;
    GameObject[] sockets;
    bool isMouseDown;
    [SerializeField] int answerNum = 0;
    [HideInInspector]public bool isStart;
    CameraController cameraController;
    GameObject player;
    public GameObject matchPuzzleDesk;
    public bool isPuzzleIsInteracting = false;
    public bool isSolved = false;
    PhotonView photonView;
    public GameObject focus;
    public SimpleInteractable hiddenHandle;
    void Awake()
    {
        if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(instance);
			return;
		}

        sockets = GameObject.FindGameObjectsWithTag("Socket");
        photonView = GetComponent<PhotonView>();
        //cam = Camera.main;
        //cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        //player = GameObject.FindGameObjectWithTag("Player");
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (GameObject.FindGameObjectWithTag("MainCamera") == null)
            yield return null;

        cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam = Camera.main;
    }


    // Update is called once per frame
    void Update()
    {
        if (!isStart) return;

        if(isPuzzleIsInteracting)
        {
            if(Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;

                Ray ray = cam.ScreenPointToRay(Input.mousePosition); //이 함수를 통해 카메라에서 mousePosition(2D좌표,z값 =0)을 관통하는 레이저가 생성됨
                if(Physics.Raycast(ray, out hit))    //ray레이저에 맞은 정보를 hit로 저장함 
                {
                    if(hit.collider.tag == "match")
                    {
                        originPos = hit.transform.position;
                        selectedObject = hit.collider.gameObject;

                        foreach(GameObject socket in sockets)
                        {
                            socket.GetComponent<BoxCollider>().enabled = true;
                        }
                                                                                                        Debug.Log(selectedObject.name);
                    }
                }
            }
            
            if(selectedObject != null)
            {
                Vector3 mousePos = Input.mousePosition;  //일반적으로 Input.mousePosition를 하면 (x,y,0)으로 리턴함
                mousePos.z = cam.WorldToScreenPoint(selectedObject.transform.position).z; // z가 0이기 때문에 worldtoscreenpoint를 이용해 카메라에서 매개변수의 좌표까지의 거리를 z값으로 설정해줌
                Vector3 worldPos = cam.ScreenToWorldPoint(mousePos); //위에서 설정한 마우스의 좌표값을 월드좌표로 변환함 
                selectedObject.transform.position = worldPos;
            }

            if (Input.GetMouseButtonUp(0)) // 마우스 버튼에서 손을 떼면
            {
                isMouseDown = false;
                SoundManager.Instance.PlaySFX("MazeMatchPut",false);
                answerNum = 0;
                if(selectedObject != null)
                    selectedObject.GetComponent<Match>().Put();

                foreach(GameObject socket in sockets)
                {
                    socket.GetComponent<MatchSocket>().CheckMatch();

                    if(socket.GetComponent<MatchSocket>().socketNum == 1 && socket.GetComponent<MatchSocket>().isHolding)
                    {
                        answerNum++;
                    }
                }
                if(answerNum == 24)
                {
                    Debug.Log("Solve Puzzle");
                    isStart = false;
                    isSolved = true;
                    focus.SetActive(true);
                    matchPuzzleDesk.transform.DOLocalMoveZ(0.4f, 2f);
                    GlobalEventManager.Instance.Publish("FocusToCharacter");
                    GlobalEventManager.Instance.Publish("ResumeCharacterControl");
                    hiddenHandle.IsInteractable = true;
                    SoundManager.Instance.PlaySFX("MazeDrawOpen", false);
                    photonView.RPC("SyncClueInteractorble", PhotonTargets.Others);
                    photonView.RPC("SyncIsSolved", PhotonTargets.Others);
                    
                }
            }

            if(isMouseDown)
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    if(selectedObject.transform.localEulerAngles.y == 270)
                    {
                        selectedObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    }
                    else if(selectedObject.transform.localEulerAngles.y == 180)
                    {
                        selectedObject.transform.localRotation = Quaternion.Euler(0, 270, 0);
                    }
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
    }
    
    [PunRPC]
    public void SyncIsStart(int ownerID)
    {
        isStart = true;
        isPuzzleIsInteracting = PhotonNetwork.player.ID == ownerID;
        
        if(isPuzzleIsInteracting)
			{
                PhotonView[] photonViews = GetComponentsInChildren<PhotonView>();
                foreach(PhotonView pv in photonViews)
                {
				    pv.TransferOwnership(PhotonNetwork.player);
                }
                matchPuzzleDesk.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player);
            }
    
    }
    
    [PunRPC]
    public void SyncUnInteracting()
    {
        isStart = false;
        isPuzzleIsInteracting = false;
    }

    [PunRPC]
    public void SyncIsSolved()
    {
        isSolved = true;
    }

    [PunRPC]
    public void SyncClueInteractorble()
    {
        hiddenHandle.IsInteractable = true; 
    }
}

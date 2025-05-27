using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using TempNamespace;
using CHG.EventDriven;
using TempNamespace.InteractableObjects;

public class StopPuzzle : MonoBehaviour
{
    public GameObject[] cubes;
    public GameObject[] startPoints;
    public GameObject[] endPoints;
    bool[] stop;
    bool[] isRight;
    public float[] answerPos = {8.75f, 8.25f};
    bool isWaiting = false;
    public bool isSolved = false;
    Ray ray;
    Camera cam;
    RaycastHit hit;
    public bool isStart = false;
    [SerializeField] GameObject StopPuzzleDesk;
    CameraController cameraController;
    GameObject player;
    PhotonView photonView;
    public bool isPuzzleIsInteracting = false;
    public GameObject focus;
    public SimpleInteractable hiddenWheel;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    IEnumerator Start()
    {
        while (GameObject.FindGameObjectWithTag("MainCamera") == null)
            yield return null;

        cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam = Camera.main;

        stop = new bool[cubes.Length];
        isRight = new bool[cubes.Length];

        for(int i = 0 ; i < cubes.Length ; i++)
        {
            stop[i] = false;
            isRight[i] = true;
            cubes[i].transform.position = startPoints[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {   
        if (!isStart) return;

        if(isPuzzleIsInteracting)
        {
            if(Input.GetMouseButtonDown(0))
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out hit))
                {
                    switch(hit.collider.tag)
                    {
                        case "StopButton1":
                            SoundManager.Instance.PlaySFX("MazeStopButtonClick", false);
                            Stop1();
                            break;
                        case "StopButton2":
                            SoundManager.Instance.PlaySFX("MazeStopButtonClick", false);
                            Stop2();
                            break;
                        case "StopButton3":
                            SoundManager.Instance.PlaySFX("MazeStopButtonClick", false);
                            Stop3();
                            break;
                    }
                }
            }
            for(int i = 0 ; i < cubes.Length ; i++)
            {   
                if(stop[i] == false)
                {    
                    if(isRight[i])
                    {
                        cubes[i].transform.Translate(Vector3.right * (1f + 0.5f * i) * 0.01f );
                    }
                    else if(!isRight[i])
                    {
                        cubes[i].transform.Translate(Vector3.left * (1f + 0.5f * i) * 0.01f );
                    }

                    if(cubes[i].transform.localPosition.x >= endPoints[i].transform.localPosition.x)
                    {
                        isRight[i] = false;
                    }
                    else if(cubes[i].transform.localPosition.x <= startPoints[i].transform.localPosition.x)
                    {
                        isRight[i] = true;
                    }
                }

            }

            if(stop[0] && stop[1] && stop[2] && !isSolved)
            {
                isWaiting = false;
                if(IsBetween(cubes[0].transform.position.z) && IsBetween(cubes[1].transform.position.z) && IsBetween(cubes[2].transform.position.z))
                {
                    isSolved = true;
                    Debug.Log("Solve Puzzle");
                    isStart = false;
                    StopPuzzleDesk.transform.DOLocalMoveX(-0.9f, 2f);
                    focus.SetActive(true);
			        GlobalEventManager.Instance.Publish("FocusToCharacter");
                    GlobalEventManager.Instance.Publish("ResumeCharacterControl");
                    hiddenWheel.IsInteractable = true;
                    SoundManager.Instance.PlaySFX("MazeDrawOpen", false);
                    photonView.RPC("SyncClueInteractorble", PhotonTargets.Others);
                    photonView.RPC("SyncIsSolved", PhotonTargets.Others);
                }
                else
                {
                    if(!isWaiting)
                    {
                        Debug.Log("Cant Solve");
                        Invoke("Restart", 0.5f);
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

    void Restart()
    {
        for(int i = 0 ; i < stop.Length ; i++)
        {
            stop[i] = false;
        }
        isWaiting = true;
    }

    bool IsBetween(float pos)
    {   
        if(pos > answerPos[1] && pos < answerPos[0])
            return true;
        else
            return false;
    }

    public void Stop1()
    {
        stop[0] = true;
    }
    public void Stop2()
    {
        stop[1] = true;
    }
    public void Stop3()
    {
        stop[2] = true;
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
                StopPuzzleDesk.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player);
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
    public void SyncIsSolved()
    {
        isSolved = true;
    }

    [PunRPC]
    public void SyncClueInteractorble()
    {
        hiddenWheel.IsInteractable = true; 
    }

}

using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using UnityEngine;
using DG.Tweening;

public class Controller : MonoBehaviour
{   
    BaseChar m_player;
    BaseChar m_target;
    Grid m_grid;
    PathFinding pathFinding;
    public Material wallMat;
    public Material selectedWallMat;
    public List<GameObject> allWalls = new List<GameObject>();
    GameObject selectedWall;
    Camera cam;
    List<GameObject> activeFalsedWall = new List<GameObject>();
    public int deleteCnt = 0;    
    public bool isStart = false;
    public bool isPuzzleIsInteracting = false;
    public bool isSolved = false;
    public bool isPopupActive = false;
    public PhotonView photonView;
    public GameObject mazePuzzleDesk;
    public GameObject focus;
    MazePuzzleExplain mazePuzzleExplain;
    
    // Start is called before the first frame update
    void Awake()
    {
        m_grid = GameObject.FindObjectOfType<Grid>();
        pathFinding = GameObject.FindObjectOfType<PathFinding>();
        m_player = GameObject.FindObjectOfType<Player>();
        m_target = GameObject.FindObjectOfType<Destination>();
        photonView = GetComponent<PhotonView>();
        mazePuzzleExplain = FindObjectOfType<MazePuzzleExplain>();
    }

    IEnumerator Start()
    {
        while (GameObject.FindGameObjectWithTag("MainCamera") == null)
            yield return null;

        cam = Camera.main;
        m_grid.CreateGrid(m_grid.m_nodeCount);
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        allWalls.AddRange(walls);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStart) return;

        if(isPuzzleIsInteracting)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit))
                {
                    if(hit.collider.CompareTag("Wall"))
                    {   
                        if(selectedWall != hit.transform.gameObject)
                        {
                            if(selectedWall != null)
                            {
                                selectedWall.GetComponent<MeshRenderer>().material = wallMat;
                            }
                            selectedWall = hit.transform.gameObject;
                            selectedWall.GetComponent<MeshRenderer>().material = selectedWallMat;
                        }                    
                    }
                }
            }
            
            if(Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log(selectedWall.GetComponent<MeshRenderer>().material.name);
                DeleteWall();
                mazePuzzleExplain.UpdateDeleteCount();
            }

            if(Input.GetKeyDown(KeyCode.R))
            {
                ResetAll();
                m_player.renderTransform.localEulerAngles = new Vector3(0f, 270f, 270f);
                mazePuzzleExplain.UpdateDeleteCount();
            }

            if(Input.GetKeyDown(KeyCode.C))
            {   
                m_player.transform.position = m_grid.FindNode(new Vector3(11.5f, 1.75f, -13.3f)).transform.position;
                m_target.transform.position = m_grid.FindNode(pathFinding.targetNodePos).transform.position;

                for(int row = 0; row < m_grid.m_nodeCount; ++row)
                {
                    for(int col = 0; col < m_grid.m_nodeCount; ++col)
                    {
                        Debug.Log("row : " + row + " col : " + col + " m_nodeCount" + m_grid.m_nodeCount);
                        m_grid.m_nodeArr[row, col].MakeNodeWall();
                    }
                }
                pathFinding.FindPathCoroutine(m_player, m_target);
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                photonView.RPC("SyncUnInteracting",PhotonTargets.All);
                m_player.renderTransform.localEulerAngles = new Vector3(0f, 270f, 270f);
                ResetAll();
                mazePuzzleExplain.UpdateDeleteCount();
                m_player.m_path.Clear();
                GlobalEventManager.Instance.Publish("FocusToCharacter");
                GlobalEventManager.Instance.Publish("ResumeCharacterControl");
                focus.SetActive(true);
                mazePuzzleExplain.gameObject.SetActive(false);
            }

            if(Input.GetKeyDown(KeyCode.Tab))
            {
                isPopupActive = !isPopupActive;
                mazePuzzleExplain.gameObject.SetActive(isPopupActive);

                if(isPopupActive)
                {
                    mazePuzzleExplain.UpdateDeleteCount();
                }
            }

            if(isSolved)
            {
                mazePuzzleDesk.transform.DOLocalMoveZ(0.3f, 2f);
            }

        }
    }

    public void DeleteWall()
    {
        if(selectedWall.GetComponent<MeshRenderer>().material.name == "SelectedWall (Instance)" && deleteCnt < 4)
        {
            int WallIndex = allWalls.IndexOf(selectedWall);
            activeFalsedWall.Add(selectedWall);
            deleteCnt++;
            mazePuzzleExplain.UpdateDeleteCount();
            photonView.RPC("DeleteWall", PhotonTargets.Others,WallIndex);
            selectedWall.SetActive(false);
        }
    }
    public void ResetAll()
    {
        pathFinding.ResetNode();
        deleteCnt = 0;
        m_player.transform.position = new Vector3(11.5f, 1.75f, -13.3f);
        foreach(GameObject obj in activeFalsedWall)
        {
            obj.SetActive(true);
            obj.GetComponent<MeshRenderer>().material = wallMat;
        }
        activeFalsedWall.Clear();
        photonView.RPC("RecoverWall", PhotonTargets.Others);
    }

    [PunRPC]
    public void SyncUnInteracting()
    {
        isStart = false;
        isPuzzleIsInteracting = false;
    }

    [PunRPC]
    public void DeleteWall(int _wallIndex)
    {
        GameObject selecetedWall  = allWalls[_wallIndex];
        activeFalsedWall.Add(selecetedWall);
        selecetedWall.SetActive(false);
    }

    [PunRPC]
    public void RecoverWall()
    {   
        pathFinding.ResetNode();
        m_player.transform.position = new Vector3(11.5f, 1.75f, -13.3f);
        foreach(GameObject falsedWall in activeFalsedWall)
        {
            falsedWall.SetActive(true);
        }
        activeFalsedWall.Clear();
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
                PhotonView[] photonViews = gameObject.transform.root.GetComponentsInChildren<PhotonView>();
                foreach(PhotonView pv in photonViews)
                {
				    pv.TransferOwnership(PhotonNetwork.player);
                }
            }
        }
    }

    [PunRPC]
    public void SyncIsSolved()
    {
        isSolved = true;
    }

}
using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using UnityEngine;

public class BaseChar : MonoBehaviour
{   
    Vector3Int intDir;
    public Vector3 Rot;
    public List<Node> m_path = new List<Node>();
    public Transform renderTransform;
    FuelManager fuelManager;
    Controller controller;
    public GameObject focus;
    public GameObject mazeExplain;
    void Awake()
    {
        fuelManager = GameObject.FindGameObjectWithTag("FuelManager").GetComponent<FuelManager>();
        controller = transform.parent.GetComponentInChildren<Controller>();
    }

    public void SetPath(List<Node> path)
    {
        if(path == null)
        {
            return;
        }
        m_path.Clear();

        foreach(Node p in path)
        {
            m_path.Add(p);
        }
    } 

    private void Update()
    {
        if(m_path.Count > 0 && fuelManager.fuelCount > 0)
        {
                                                                                                            //Debug.Log("move");
            Vector3 dir = m_path[0].transform.position - transform.position;
            dir.Normalize();

            transform.Translate(dir * 0.01f);
            float distance = Vector3.Distance(m_path[0].transform.position, transform.position);

            if(m_path.Count > 1 && fuelManager.fuelCount > 1)
            {
                intDir = Vector3Int.RoundToInt(dir);
                
                if (intDir == new Vector3Int(-1, 0, 0))                     // 오른쪽
                {
                    renderTransform.localEulerAngles = new Vector3(0f, 270f, 270f);
                }
                else if (intDir == new Vector3Int(1, 0, 0))                 // 왼쪽
                {
                    renderTransform.localEulerAngles = new Vector3(180f, 270f, 270f);
                }
                else if (intDir == new Vector3Int(0, 1, 0))                 // 위쪽
                {
                    renderTransform.localEulerAngles = new Vector3(270f, 270f, 270f);
                }
                else if (intDir == new Vector3Int(0, -1, 0))                // 아래쪽
                {
                    renderTransform.localEulerAngles = new Vector3(90f, 270f, 270f);
                }
            }
            else
            {
                //renderTransform.localEulerAngles = new Vector3(0f, 270f, 270f);
            }
            
            if(distance < 0.05f )
            {
                m_path.RemoveAt(0);
                fuelManager.fuelCount--;
                fuelManager.UpdateFuelUI();
                fuelManager.photonView.RPC("DecreaseFuelCount",PhotonTargets.Others);

            }
            if(m_path.Count < 1)
            {
                Debug.Log("Solve");
                controller.isSolved = true;
                controller.photonView.RPC("SyncIsSolved",PhotonTargets.Others);
                GlobalEventManager.Instance.Publish("ResumeCharacterControl");
                GlobalEventManager.Instance.Publish("FocusToCharacter");
                SoundManager.Instance.PlaySFX("MazeDrawOpen", false);
                focus.SetActive(true);
                if(mazeExplain.activeSelf)
                    mazeExplain.SetActive(false);
            }
        }
    }
}

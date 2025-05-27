using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : MonoBehaviour
{
    public Collider other;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerEnter(Collider col)
    {
        other = col;
    }
    void OnTriggerExit(Collider col)
    {
        if(other == col) other = null;
    }
    public void Put()
    {
        if(MatchPuzzle.instance.selectedObject == this.gameObject)
        {
            if(other == null || other.tag != "Socket" || other.GetComponent<MatchSocket>().isHolding)
            {
                transform.position = MatchPuzzle.instance.originPos;
                MatchPuzzle.instance.selectedObject = null;
                //GetComponent<PhotonView>().RPC("SyncMatchTransform", PhotonTargets.Others, transform.position, transform.rotation);
                SocketManager.instance.SetUnableAllColliderSockets();                
            }
            else if(other.tag == "Socket")
            {
                transform.position = other.transform.position;
                MatchPuzzle.instance.selectedObject = null;
                //GetComponent<PhotonView>().RPC("SyncMatchTransform", PhotonTargets.Others, transform.position, transform.rotation);
                SocketManager.instance.SetUnableAllColliderSockets();
            }
        } 
    }

    [PunRPC]
    public void SyncMatchTransform(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MatchSocket : MonoBehaviour
{  
    public GameObject match = null;
    public bool isHolding = false;
    public int socketNum;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    void Start()
    {
        CheckMatch();
    }

    // Update is called once per frame
    void Update()
    {
            
    }

    public void CheckMatch()  
    {
        isHolding = false;
        
        Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(GetComponent<BoxCollider>().center), GetComponent<BoxCollider>().size / 2, 
                                                    transform.rotation, LayerMask.GetMask("Default"),QueryTriggerInteraction.Collide);

        foreach(Collider col in colliders)
        {
            if(col.tag == "match")
            {
                isHolding = true;
                match = col.gameObject;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(match == null)
        {
            match = col.gameObject;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(match == col.gameObject)
        {
            match = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;  
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(GetComponent<BoxCollider>().center), transform.rotation, transform.localScale);  
        Gizmos.DrawWireCube(Vector3.zero, transform.GetComponent<BoxCollider>().size);  
    }
}

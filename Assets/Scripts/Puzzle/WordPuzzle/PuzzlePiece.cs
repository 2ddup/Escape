using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{   
    public Collider col = null;
    
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {   
        if(col == null)
            col = other;
    }
    void OnTriggerExit(Collider other)
    {
        if(other == col)
            col = null;
    }

}

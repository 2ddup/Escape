using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 
public class Drawer : MonoBehaviour
{   
    bool isDrag = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Drag()
    {
        if(!isDrag)
        {
            transform.DOLocalMoveZ(0.6f, 0.5f);
            isDrag = true;
        }
        else
        {
            transform.DOLocalMoveZ(0.254f, 0.5f);
            isDrag = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkListPopup : MonoBehaviour
{
    public GameObject redLens;
    public GameObject blueLens;

    public Animator listPopupAni;
    

    //리스트 애니메이션이 이미 실행되었는가?
    private bool renLensAni = false;
    private bool blueLensAni = false;
    private bool clear = false;

    // Update is called once per frame
    void Update()
    {
        if(!renLensAni && redLens != null && !redLens.GetComponent<Collider>().enabled)
        {
            Debug.Log("Trigger set!");
            listPopupAni.SetTrigger("redDisappear");
            renLensAni = true;
        }

        if(!blueLensAni && blueLens != null && !blueLens.GetComponent<Collider>().enabled)
        {
            Debug.Log("Trigger set!");
            listPopupAni.SetTrigger("blueDisappear");
            blueLensAni = true;
        }

        if(renLensAni && blueLensAni && !clear)
        {
            Debug.Log("Trigger set!");
            listPopupAni.SetTrigger("Clear");
            clear = true;
        }
    }
}

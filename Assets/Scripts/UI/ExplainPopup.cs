using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplainPopup : MonoBehaviour
{
    public GameObject popupUI;
    public GameObject[] otherUIparents;

    private Animator popupAni;
    private Controller controller;

    private bool isPopupActive = true;
    private bool hasInteracted = false;

    void Awake()
    {
        popupAni = popupUI.GetComponent<Animator>();
        controller = FindObjectOfType<Controller>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(popupUI != null)
        {
            popupUI.SetActive(true);

            if(popupAni != null)
            {
                popupAni.SetTrigger("Play");
            }

            SetOtherUIIsActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(controller == null || (controller!=null &&!controller.isStart))
            {
                isPopupActive = !isPopupActive;
                popupUI.SetActive(isPopupActive);

                if(!isPopupActive && !hasInteracted)
                {
                    SetOtherUIIsActive(true);
                    hasInteracted = true;
                }
            }
        }
    }

    void SetOtherUIIsActive(bool active)
    {
        foreach(GameObject otherUI in otherUIparents)
        {
            if(otherUI != null)
               otherUI.SetActive(active);
        }
    }
}

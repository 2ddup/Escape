using System.Collections;
using System.Collections.Generic;
using TempNamespace.Character.Controller;
using UnityEngine;

public class GravityCtrl : MonoBehaviour
{
    public GameObject player;
    private FirstPersonController playerCtrl;

    private bool isOnQuad = false;
    private bool isButtPressed = false;
    private bool isReversed = false;

    void Awake()
    {
        if(player != null)
        {
            playerCtrl = player.GetComponent<FirstPersonController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isOnQuad && isButtPressed)
        {
            if(!isReversed)
            {
                isReversed = true;
                playerCtrl.gravityScale = -1f;
                player.transform.rotation = Quaternion.Euler(180f, player.transform.rotation.y, player.transform.rotation.z);

            }
        }
        else{
            if(isReversed)
            {
                isReversed = false;
                playerCtrl.gravityScale = 1f;
                player.transform.rotation = Quaternion.Euler(0f, player.transform.rotation.y, player.transform.rotation.z);

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Quad"))
        {
            isOnQuad = true;
        }

        if(other.CompareTag("Button"))
        {
            isButtPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Quad"))
        {
            isOnQuad = false;
        }

        if(other.CompareTag("Button"))
        {
            isButtPressed = false;
        }
    }
}

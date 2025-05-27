using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusActive : MonoBehaviour
{
    public GameObject focus;

    // Start is called before the first frame update
    void Start()
    {
        focus.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            bool currentState = focus.activeSelf;
            focus.SetActive(!currentState);
        }
    }
}

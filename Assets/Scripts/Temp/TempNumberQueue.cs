using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TempNumberQueue : MonoBehaviour
{
    public bool b1;
    public bool b2;
    public bool b3;

    public MeshRenderer l1;
    public MeshRenderer l2;
    public MeshRenderer l3;

    public Material red;
    public Material green;
    public UnityEvent ond;
    void OnDestroy()
    {
        ond.Invoke();
    }
    public void PushB1()
    {
        if(!b1)
        {
            b1 = true;
            l1.material = green;
        }
    }
    public void PushB2()
    {
        if(!b1)
        {
            Wrong();
        }
        else if(!b2)
        {
            b2 = true;
            l2.material = green;
        }
    }
    public void PushB3()
    {
        if(!b1 || !b2)
        {
            Wrong();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    void Wrong()
    {
        
        b1 = false;
        b2 = false;
        b3 = false;

        l1.material = red;
        l2.material = red;
        l3.material = red;
    }
    public void PushNumber(int number)
    {
        if(number == 1)
        {
            b1 = true;
            l1.material = green;
        }
        if(number == 2)
        {
            b2 = true;
            l2.material = green;
        }
        if(number == 3)
        {
            b3 = true;
            l3.material = green;
        }

        if(b1 && b2 && b3)
            Destroy(this.gameObject);
    }
    public void OffNumber(int number)
    {
        if(number == 1)
        {
            b1 = false;
            l1.material = red;
        }
        if(number == 2)
        {
            b2 = false;
            l2.material = red;
        }
        if(number == 3)
        {
            b3 = false;
            l3.material = red;
        }
    }
}

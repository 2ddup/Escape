using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hello : MonoBehaviour
{
    bool hello = false;
    public void Hell_O()
    {
        hello = true;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if(hello)
        {
            transform.Translate(Vector3.back * Time.fixedDeltaTime * 2.5f);

            if(transform.localPosition.z <= 6f)
                hello = false;
        }
    }
}

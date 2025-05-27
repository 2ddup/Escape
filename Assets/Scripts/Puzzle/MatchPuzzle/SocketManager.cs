using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public static SocketManager instance;
    public GameObject[] sockets;

    void Awake()
    {
        if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(instance);
			return;
		}
        
        sockets = GameObject.FindGameObjectsWithTag("Socket");
                                                                            Debug.Log(instance);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetAbleColliderAllSockets()
    {
                                                                            Debug.Log("Collider 활성화 호출");
        foreach(GameObject socket in sockets)
        {
            socket.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void SetUnableAllColliderSockets()
    {
                                                                            Debug.Log("Collider 비활성화 호출");
        foreach(GameObject socket in sockets)
        {
            socket.GetComponent<BoxCollider>().enabled = false;
        }
    }
}

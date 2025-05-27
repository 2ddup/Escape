using System.Collections;
using System.Collections.Generic;
using TempNamespace.Character;
using UnityEngine;
using UnityEngine.Events;

public class Fun : MonoBehaviour
{
    public Vector3 targetPosition;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
    }

    public UnityEvent OnDoomed;

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.isWriting)
        {
        }
        else
        {
        }
    }

    public void Force()
    {
        GetComponent<PhotonView>().RPC("Place", PhotonTargets.AllBuffered);
    }
    public AudioClip fun;
    [PunRPC]
    void Place()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Character>().transform.position = targetPosition;
        OnDoomed?.Invoke();
        SoundManager.Instance.PlaySFX("HOHO HAHA");
        SoundManager.Instance.PlayBGM(fun);

        PlayerPrefs.SetInt("Clear", 1);
    }
}

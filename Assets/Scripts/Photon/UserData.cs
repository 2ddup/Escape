using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserData : MonoBehaviour
{
    public TMP_InputField userId;

    public Sprite[] readyImgs;
    Image readyButtonImg;
    PhotonPlayer player;
    PhotonInit photonInit;

    void Awake()
    {
        readyButtonImg = transform.Find("Button").GetComponent<Image>();
        photonInit = GameObject.FindGameObjectWithTag("PhotonInit").GetComponent<PhotonInit>();
    }
    // Start is called before the first frame update
    void Start()
    {
        readyButtonImg.sprite = readyImgs[0];
    }

    public void OnClickReadyButton()
    {
        if (!player.IsLocal) return;

        if(readyButtonImg.sprite == readyImgs[0])
            readyButtonImg.sprite = readyImgs[1];
        else
            readyButtonImg.sprite = readyImgs[0];
        
        photonInit.SetReady();
    }

    public void SetUserID(PhotonPlayer _player)
    {
        this.player = _player;
        userId.text = player.NickName;
    }

    public void UpdateReadyState()
    {
        object isReady;
        if (player.CustomProperties.TryGetValue("isReady", out isReady))
        {
            readyButtonImg.sprite = (bool)isReady ? readyImgs[1] : readyImgs[0];
        }
    }

    public bool IsPlayer(PhotonPlayer _player)
    {
        return player == _player;
    }

}

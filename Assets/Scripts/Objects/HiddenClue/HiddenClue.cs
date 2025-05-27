using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenClue : MonoBehaviour, IHiddenObject
{   
    public enum ClueType {Door, Wheel, Handle}
    public ClueType clueType;
    PhotonView photonView;
    MazeCameraHolder mazeCameraHolder;
    Animator listAnimator;
    Item item;
    int itemId;
    public void Awake()
    {
        photonView = GetComponent<PhotonView>();
        mazeCameraHolder = FindObjectOfType<MazeCameraHolder>();
        listAnimator = FindObjectOfType<MazeListPopup>().GetComponent<Animator>();
        item = GetComponent<Item>();
    }
    private void OnEnable()
    {
        Item.OnItemPickedUp += GetItem;
    }

    private void OnDisable()
    {
        Item.OnItemPickedUp -= GetItem;
    }

    public void GetItem(int _itemId, int __)
    {
        if(item.pickedItemObject != this.gameObject) return;
        mazeCameraHolder.hiddenClueCount++;

        switch(clueType)
        {
            case ClueType.Door:
                listAnimator.SetTrigger("Door");
                break;
            case ClueType.Wheel:
                listAnimator.SetTrigger("Wheel");
                break;
            case ClueType.Handle:
                listAnimator.SetTrigger("Handle");
                break;
        }
        
        SoundManager.Instance.PlaySFX("PickItem", false);
        photonView.RPC("GetHiddenItem", PhotonTargets.Others);
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void GetHiddenItem()
    {
        mazeCameraHolder.hiddenClueCount++;
        
        switch(clueType)
        {
            case ClueType.Door:
                listAnimator.SetTrigger("Door");
                break;
            case ClueType.Wheel:
                listAnimator.SetTrigger("Wheel");
                break;
            case ClueType.Handle:
                listAnimator.SetTrigger("Handle");
                break;
        }

        gameObject.SetActive(false);
    }
}

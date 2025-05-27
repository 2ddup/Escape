using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelTank5 : MonoBehaviour, IHiddenObject
{
    PhotonView photonView;
    FuelManager fuelManager;
    Item item;
    public void Awake()
    {
        photonView = GetComponent<PhotonView>();
        fuelManager = GetComponentInParent<FuelManager>();
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

    public void GetItem(int itemId, int __)
    {
        if(item.pickedItemObject != this.gameObject) return;
        
        fuelManager.fuelCount += 5;
        SoundManager.Instance.PlaySFX("PickItem", false);
        photonView.RPC("GetFuelTank5",PhotonTargets.Others);
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void GetFuelTank5()
    {
        fuelManager.fuelCount += 5;
        gameObject.SetActive(false);
    }
}

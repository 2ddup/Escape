using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelTank1 : MonoBehaviour, IHiddenObject
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

        fuelManager.fuelCount += 1;
        SoundManager.Instance.PlaySFX("PickItem", false);
        photonView.RPC("GetFuelTank1",PhotonTargets.Others);
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void GetFuelTank1()
    {
        fuelManager.fuelCount += 1;
        gameObject.SetActive(false);
    }
}

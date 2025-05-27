using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelManager : MonoBehaviour
{   
    public int fuelCount = 0;
    public PhotonView photonView;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    [PunRPC]
    public void DecreaseFuelCount()
    {
        fuelCount--;
        UpdateFuelUI();
    }

    public void UpdateFuelUI()
    {
        // 연료 수치가 변경되었으므로 UI에 갱신을 알려줍니다.
        InventoryManager.instance.UpdateFuelUI(fuelCount);
    }
}

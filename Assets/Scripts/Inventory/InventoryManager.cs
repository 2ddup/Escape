using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TempNamespace.Player;

// NO DB

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    // 두 개의 인자를 받는 이벤트 정의
    public delegate void OnInventoryUpdated(List<ItemData> items, int fuelCount);
    public event OnInventoryUpdated onInventoryUpdated;

    private List<ItemData> currentInventory = new List<ItemData>();

    public ItemDatabase itemDatabase;

    public int playerID;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    IEnumerator Start()
    {
        while (GameObject.FindGameObjectsWithTag("Player").Length < 2)
            yield return null;

        playerID = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PhotonNetworkPlayer>().PlayerID;
        Debug.Log("My Player ID: " + playerID);
    }

    private void OnEnable()
    {
        Item.OnItemPickedUp += AddItem;
    }

    private void OnDisable()
    {
        Item.OnItemPickedUp -= AddItem;
    }

    public void SetPlayerId(int playerId)
    {
        playerID = playerId;
    }

    // FuelManager에서 fuelCount를 받아오는 함수
    public int GetFuelCount()
    {
        FuelManager fuelManager = FindObjectOfType<FuelManager>();
        return fuelManager != null ? fuelManager.fuelCount : 0;
    }

    public void UpdateFuelUI(int fuelCount)
    {
        // 연료 수치가 변경되었으므로 UI에 갱신을 알려줍니다.
        onInventoryUpdated?.Invoke(currentInventory, fuelCount);
    }

    public void AddItem(int id, int quantity)
    {
        ItemData baseItem = itemDatabase.GetItemDataById(id);
        if (baseItem == null)
        {
            Debug.LogWarning($"[InvenManager] itemDatabase에 id {id} 해당 아이템 없음");
            return;
        }

        // 기존에 있던 아이템이면 수량만 증가
        ItemData existing = currentInventory.Find(i => i.item_id == id);
        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            // 복사본 생성
            ItemData newItem = new ItemData
            {
                item_id = baseItem.item_id,
                item_name = baseItem.item_name,
                prefab_name = baseItem.prefab_name,
                item_type = baseItem.item_type,
                quantity = quantity
            };

            currentInventory.Add(newItem);
        }

        int fuelCount = GetFuelCount();  // Fuel의 현재 수량을 가져옴
        onInventoryUpdated?.Invoke(currentInventory, fuelCount);  // Fuel 수량도 같이 전달

        StartCoroutine(DelayedUpdate());
    }

    private IEnumerator DelayedUpdate()
    {
        yield return null; // 한 프레임 대기
        int fuelCount = GetFuelCount();
        onInventoryUpdated?.Invoke(currentInventory, fuelCount);
    }

    public void RemoveItem(int itemId)
    {
        ItemData existing = currentInventory.Find(i => i.item_id == itemId);
        if (existing != null)
        {
            currentInventory.Remove(existing);

            // 연료 감소 처리
            UpdateFuelUI(GetFuelCount());
            StartCoroutine(DelayedUpdate());
        }
    }

    public void UseItem(int itemId)
    {
        ItemData existing = currentInventory.Find(i => i.item_id == itemId);
        if (existing != null)
        {
            existing.quantity--;
            if (existing.quantity <= 0)
                currentInventory.Remove(existing);

             // 연료 감소 처리
            UpdateFuelUI(GetFuelCount());
            StartCoroutine(DelayedUpdate());
        }
    }

    public List<ItemData> GetInventory()
    {
        return currentInventory;
    }
}

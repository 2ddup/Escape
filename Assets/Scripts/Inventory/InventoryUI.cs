using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public Transform slotParent;

    [Header("Settings")]
    public int maxSlotCount = 5;

    private bool isOpen = true;
    private List<ItemData> currentItems = new List<ItemData>();
    private ItemData equippedItem = null;

    private static InventoryUI instance;
    public static event Action<int> OnItemUse;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.onInventoryUpdated -= UpdateInventoryUI;
            InventoryManager.instance.onInventoryUpdated += UpdateInventoryUI;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.onInventoryUpdated -= UpdateInventoryUI;
        }
    }

    private void Start()
{
    inventoryPanel.SetActive(isOpen);

    if (InventoryManager.instance != null)
    {
        InventoryManager.instance.onInventoryUpdated -= UpdateInventoryUI;
        InventoryManager.instance.onInventoryUpdated += UpdateInventoryUI;

        // Fuel 수량도 전달받아야 하므로 임시 변수 선언해서 호출 필요
        var inventory = InventoryManager.instance.GetInventory();
        int fuelCount = InventoryManager.instance.GetFuelCount(); // ← 이 메서드가 필요
        UpdateInventoryUI(inventory, fuelCount);
    }
}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);

            if (isOpen && InventoryManager.instance != null)
            {
                List<ItemData> inventory = InventoryManager.instance.GetInventory();
                int fuelCount = InventoryManager.instance.GetFuelCount(); // 이 메서드가 존재해야 함
                UpdateInventoryUI(inventory, fuelCount);
            }
        }

        for (int i = 0; i < maxSlotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (i < currentItems.Count)
                {
                    int tempItemId = currentItems[i].item_id;
                    UseItem(currentItems[i]);
                    OnItemUse?.Invoke(tempItemId);
                }
            }
        }
    }

    private void UpdateInventoryUI(List<ItemData> items, int fuelCount)
    {
        Debug.Log($"[InventoryUI] UI 업데이트 요청됨 - 아이템 수: {items.Count}, Fuel 수량: {fuelCount}");
        currentItems.Clear();

        HashSet<int> addedItemIds = new HashSet<int>();
        bool fuelAdded = false;

        foreach (var item in items)
        {
            if (equippedItem != null && item.item_id == equippedItem.item_id)
                continue;

            if (item.prefab_name == "Fuel")
            {
                if (!fuelAdded)
                {
                    ItemData fuelCopy = new ItemData
                    {
                        item_id = item.item_id,
                        item_name = item.item_name,
                        prefab_name = item.prefab_name,
                        item_type = item.item_type,
                        quantity = fuelCount  // ✅ 실제 Fuel 수량을 여기에 반영
                    };

                    currentItems.Add(fuelCopy);
                    fuelAdded = true;
                }
            }
            else
            {
                if (!addedItemIds.Contains(item.item_id))
                {
                    currentItems.Add(item);
                    addedItemIds.Add(item.item_id);
                }
            }
        }

        RefreshInventoryUI();
    }

    private void UseItem(ItemData item)
    {
        if (item.prefab_name == "Fuel" || item.prefab_name == "HiddenDoor" ||
            item.prefab_name == "Truck_Handle" || item.prefab_name == "Truck_Wheel_AR")
        {
            Debug.Log("해당 아이템은 인벤토리에서 직접 사용할 수 없습니다.");
            return;
        }

        if (item.item_type == "equipment")
        {
            if (equippedItem != null && equippedItem.item_id == item.item_id)
                return;

            if (equippedItem != null)
            {
                currentItems.Add(equippedItem);
                Debug.Log($"[InventoryUI] 기존 장비 {equippedItem.item_name}를 다시 인벤토리에 추가");
            }

            equippedItem = item;
            currentItems.RemoveAll(x => x.item_id == item.item_id);
        }
        else
        {
            item.quantity--;
            if (item.quantity <= 0)
                currentItems.RemoveAll(x => x.item_id == item.item_id);

            InventoryManager.instance.UseItem(item.item_id);
        }

        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currentItems.Count && i < maxSlotCount; i++)
        {
            CreateSlot(currentItems[i]);
        }
    }

    private void CreateSlot(ItemData item)
    {
        GameObject slot = Instantiate(slotPrefab, slotParent);

        TextMeshProUGUI nameText = slot.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI quantityText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();

        if (nameText != null) nameText.text = item.item_name;

        if (iconImage != null)
        {
            Sprite icon = Resources.Load<Sprite>($"Icons/{item.prefab_name}");
            if (icon != null) iconImage.sprite = icon;
        }

        if (quantityText != null)
        {
            if (item.prefab_name == "Fuel")
            {
                quantityText.text = $"x{item.quantity}";
            }
            else
            {
                quantityText.text = item.item_type == "equipment" ? "" : $"x{item.quantity}";
            }
        }
    }
}

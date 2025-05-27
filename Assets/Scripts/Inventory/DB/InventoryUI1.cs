using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventoryUI1 : MonoBehaviour
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

    private static InventoryUI1 instance;
    public static event Action<int> OnItemUse;

    private void Awake()
    {
        // 싱글톤 처리 + DontDestroy
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        if (InventoryManager1.instance != null)
        {
            InventoryManager1.instance.onInventoryUpdated -= UpdateInventoryUI;
            InventoryManager1.instance.onInventoryUpdated += UpdateInventoryUI;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager1.instance != null)
        {
            InventoryManager1.instance.onInventoryUpdated -= UpdateInventoryUI;
        }
    }

    private void Start()
    {
        //inventoryPanel.SetActive(true);

        if (InventoryManager1.instance != null)
        {
            InventoryManager1.instance.onInventoryUpdated -= UpdateInventoryUI;
            InventoryManager1.instance.onInventoryUpdated += UpdateInventoryUI;
            InventoryManager1.instance.ResetInventory();  // 내부에서 플레이어 ID 사용
            InventoryManager1.instance.GetInventory();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);

            // 열었을 때 항상 갱신
            if (isOpen && InventoryManager1.instance != null)
            {
                InventoryManager1.instance.GetInventory();
            }
        }

        for (int i = 0; i < maxSlotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (i < currentItems.Count)
                {
                    int tempItemId = currentItems[i].item_id;
                    RemoveItemSlot(currentItems[i]);
                    OnItemUse?.Invoke(tempItemId);
                }
            }
        }
    }

    private void UpdateInventoryUI(List<ItemData> items)
    {
        Debug.Log($"인벤토리 UI 갱신 요청: 아이템 {items.Count}개");
        
        // 장비 제외한 목록만 currentItems에 복사
        currentItems.Clear();

        foreach (var item in items)
        {
            if (equippedItem == null || item.item_id != equippedItem.item_id)
            {
                currentItems.Add(item);
            }
        }

        RefreshInventoryUI();
    }

    private void RemoveItemSlot(ItemData item)
    {   
        // Fuel은 인벤토리에서 직접 사용하지 않음
        if (item.prefab_name == "Fuel" || item.prefab_name == "HiddenDoor" || item.prefab_name == "Truck_Handle" || item.prefab_name == "Truck_Wheel_AR")
        {
            Debug.Log("해당 아이템은 인벤토리에서 직접 사용할 수 없습니다.");
            return;
        }

        if (item.item_type == "equipment")
        {
            // 동일 장비일 경우 무시
            if (equippedItem != null && equippedItem.item_id == item.item_id)
                return;

            // 기존 장비 다시 인벤토리에 추가
            if (equippedItem != null)
            {
                currentItems.Add(equippedItem);
                Debug.Log($"기존 장비 {equippedItem.item_name}를 인벤토리에 추가");
            }

            // 새 장비 장착
            equippedItem = item;
            currentItems.RemoveAll(x => x.item_id == item.item_id);

            // 이벤트 전달
            //GlobalEventManager.Instance.Publish("Equip", new EquipArgs(0, null));
            Debug.Log($"장비 장착: {item.item_name}");
        }
        else
        {
            item = currentItems.Find(x => x.item_id == item.item_id);
            if (item != null)
            {
                item.quantity--;
                if (item.quantity <= 0)
                {
                    currentItems.RemoveAll(x => x.item_id == item.item_id);
                }

                RefreshInventoryUI();
            }

            InventoryManager1.instance.RemoveQuantity(item.item_id);
            Debug.Log($"소모품 사용: {item.item_name}");
        }

        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {
        foreach (Transform child in slotParent)
        {
            if (child != null)
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

        // 슬롯 안에 있는 TextMeshPro, Image 컴포넌트들 직접 설정 (비활성화시 find 안됨)
        TextMeshProUGUI nameText = slot.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI quantityText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();

        if (nameText != null)
            nameText.text = item.item_name;

        if (iconImage != null)
        {
            Sprite icon = Resources.Load<Sprite>($"Icons/{item.prefab_name}");
            if (icon != null)
                iconImage.sprite = icon;
        }

        if (quantityText != null)
        {
            if(item.prefab_name == "Fuel")
            {
                // FuelManager에서 현재 수량 가져와 표시
                FuelManager fuelManager = FindObjectOfType<FuelManager>();
                int fuelCount = fuelManager != null ? fuelManager.fuelCount : item.quantity;
                quantityText.text = $"x{fuelCount}";
            }
            else
            {
               quantityText.text = item.item_type == "equipment" ? "" : $"x{item.quantity}"; 
            }
        }
    }
}

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] items; // 아이템들을 저장할 배열

    // 아이템을 ID로 찾는 메서드 (아이템 ID로 검색 가능)
    public ItemData GetItemDataById(int id)
    {
        foreach (var item in items)
            {
                if (item.item_id == id)
                {
                    return item;
                }
            }
            return null; // 아이템을 찾을 수 없는 경우 null 반환
    }

    // 아이템을 이름으로 찾는 메서드 (아이템 이름으로 검색 가능)
    public ItemData GetItemByName(string name)
    {
        foreach (var item in items)
        {
            if (item.item_name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }
        return null; // 아이템을 찾을 수 없는 경우 null 반환
    }
}

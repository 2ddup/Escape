using System;

[Serializable]
public class ItemData
{
    public int item_id;
    public string item_name;
    public string item_type; // 예: "equipment", "consumable"
    public int quantity;
    public string prefab_name; // 아이콘 경로에 사용될 이름
}


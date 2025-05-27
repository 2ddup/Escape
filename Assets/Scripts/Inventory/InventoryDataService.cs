using System;
using System.Collections.Generic;
using UnityEngine;

public static class InventoryDataService
{
    public static List<ItemData> ParseInventory(string json)
    {
        try
        {
            InventoryResponse response = JsonUtility.FromJson<InventoryResponse>(json);
            return response?.inventory ?? new List<ItemData>();
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 파싱 오류: {e.Message}");
            return new List<ItemData>();
        }
    }
}

[Serializable]
public class InventoryResponse
{
    public List<ItemData> inventory;
}

[Serializable]
public class ErrorResponse
{
    public string error;
}

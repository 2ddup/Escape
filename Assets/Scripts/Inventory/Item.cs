using UnityEngine;
using System;
using TempNamespace.InteractableObjects;

public class Item : MonoBehaviour
{
    public static event Action<int, int> OnItemPickedUp;

    public int itemId;
    public int quantity;
    public GameObject pickedItemObject;

    void Awake()
    {
        Debug.Log($"{gameObject.name} has itemId {itemId}");
    }
    public void ItemPickup()
    {
        Debug.Log($"아이템 {itemId} 획득됨");
        pickedItemObject = this.gameObject;
        OnItemPickedUp?.Invoke(itemId, quantity = 1);
    }
}

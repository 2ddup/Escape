using System;
using System.Collections;
using System.Collections.Generic;
using TempNamespace.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class InventoryManager1 : MonoBehaviour
{
    public static InventoryManager1 instance;

    public delegate void OnInventoryUpdated(List<ItemData> items);
    public event OnInventoryUpdated onInventoryUpdated;

    // public delegate void OnItemUsed(ItemData item);
    // public event OnItemUsed onItemUsed;

    private const string baseUrl = "http://192.168.0.18:8080/inventory.php";

    private List<ItemData> currentInventory = new List<ItemData>();
    public int playerID; // 각 플레이어 고유 ID


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        while (GameObject.FindGameObjectsWithTag("Player").Length < 2)
        yield return null;

        playerID = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PhotonNetworkPlayer>().PlayerID;
        Debug.Log("My Player ID: " + playerID);

        ResetInventory();
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

    public void ResetInventory()
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "reset_inventory");
        form.AddField("player_id", playerID);

        UnityWebRequest request = UnityWebRequest.Post(baseUrl, form);
        StartCoroutine(SendRequest(request, "ResetInventory"));
    }

    public void GetInventory()
    {
        string url = $"{baseUrl}?action=get_inventory&player_id={playerID}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        StartCoroutine(SendRequest(request, "GetInventory"));
    }

    public void AddItem(int itemId, int quantity = 1)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "add_item");
        form.AddField("player_id", playerID);
        form.AddField("item_id", itemId);
        form.AddField("quantity", quantity);

        UnityWebRequest request = UnityWebRequest.Post(baseUrl, form);
        StartCoroutine(SendRequest(request, "AddItem"));
    }

    public void RemoveItem(int itemId)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "remove_item");
        form.AddField("player_id", playerID);
        form.AddField("item_id", itemId);

        UnityWebRequest request = UnityWebRequest.Post(baseUrl, form);
        StartCoroutine(SendRequest(request, "RemoveItem"));
    }
    public void RemoveQuantity(int itemId) // 이슬비 추가
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "remove_quantity");
        form.AddField("player_id", playerID);
        form.AddField("item_id", itemId);

        UnityWebRequest request = UnityWebRequest.Post(baseUrl, form);
        StartCoroutine(SendRequest(request, "RemoveQuantity"));
    }

    public void UseItem()
    {
    }

    IEnumerator SendRequest(UnityWebRequest request, string actionName)
    {
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.Success)
#else
        if (!request.isNetworkError && !request.isHttpError)
#endif
        {
            string response = request.downloadHandler.text;
            Debug.Log($"{actionName} 응답: {response}");

            if (response.Contains("error"))
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(response);
                Debug.LogWarning($"{actionName} 오류: {error.error}");
                yield break;
            }

            if (actionName == "GetInventory")
            {
                currentInventory = InventoryDataService.ParseInventory(response);
                onInventoryUpdated?.Invoke(currentInventory);
            }
            else if (actionName == "AddItem" || actionName == "RemoveItem")
            {
                // 아이템 추가/삭제 후 인벤토리 새로 받아오기
                GetInventory();
            }
        }
        else
        {
            Debug.LogError($"{actionName} 네트워크 오류: {request.error}");
        }
    }
}

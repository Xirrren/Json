using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleSheetManager : MonoBehaviour
{
    private string googleSheetUrl = "https://script.google.com/macros/s/AKfycbxU4WDjwZQaiC3ToFDBd_UPKm4dk8wnNdlY5LLKZNorUzGWPIs_gNuam1OHuk39pJYx/exec"; // 這是你的 URL

    // 用來傳送購買資料到 Google Sheet
    public void SendDataToGoogleSheet(ItemData item)
    {
        // 創建一個要送出的資料
        var postData = new PostData
        {
            id = item.ID,
            name = item.Name,
            price = item.Price,
            description = item.Description,
            quantity = item.Quantity,
            spriteName = item.SpriteName
        };

        // 轉換成 JSON 格式
        string jsonData = JsonUtility.ToJson(postData);
        
        // 開始發送請求
        StartCoroutine(PostRequest(googleSheetUrl, jsonData));
    }
    public void UpdateItemToSheet(ItemData item)
    {
        var updateData = new
        {
            type = "updateItem",
            name = item.Name,
            quantity = item.Quantity
        };

        string jsonData = JsonUtility.ToJson(updateData);
        StartCoroutine(PostRequest(googleSheetUrl, jsonData));
    }
    public void UpdateItemQuantity(string itemName, int newQuantity)
    {
        var updateData = new UpdateItemRequest
        {
            name = itemName,
            quantity = newQuantity
        };

        string jsonData = JsonUtility.ToJson(updateData);
        StartCoroutine(PostRequest(googleSheetUrl, jsonData));
    }
    public void UpdatePlayerCoins(string playerId, int newCoinValue)
    {
        var updateData = new UpdateCoinsRequest
        {
            playerId = playerId,
            coins = newCoinValue
        };

        string jsonData = JsonUtility.ToJson(updateData);
        StartCoroutine(PostRequest(googleSheetUrl, jsonData));
    }
    // 發送 POST 請求
    private IEnumerator PostRequest(string url, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 等待請求完成
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data successfully sent to Google Sheet");
        }
        else
        {
            Debug.LogError("Error sending data: " + request.error);
        }
    }

    // 用來轉換 POST 資料的類
    [System.Serializable]
    public class PostData
    {
        public string id;
        public string name;
        public float price;
        public string description;
        public int quantity;
        public string spriteName;
    }
}

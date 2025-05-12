using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ShopManager1 : MonoBehaviour
{
    public Text coinText;
    public string playerId = "P001"; // ✅ 在這裡定義玩家 ID
    public GameObject itemPrefab;
    public Transform itemContainer;
    public int playerCoins = 5000;
    private string filePath;
    private ItemDataList itemDataList;

    [Header("Google Sheet API")]
    public string url = "https://script.google.com/macros/s/AKfycbxBG935lMTjAEyLn8XdSWve-9x4b_E5K1qRmBT12Vzlvtddc8jkZZ7-o8wthu7Op18/exec";  // 貼上你的 Google Apps Script 網址
    private GoogleSheetManager googleSheetManager;  // 新增 GoogleSheetManager
    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "shopData.json");
        googleSheetManager = GetComponent<GoogleSheetManager>(); //新增
        StartCoroutine(LoadDataFromWeb());
    }

    IEnumerator LoadDataFromWeb()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log($"Loaded JSON from Web: {json}"); 
            try
            {
            
            itemDataList = JsonConvert.DeserializeObject<ItemDataList>(json);
            Debug.Log($"Item Data List Count: {itemDataList.Items.Count}");  // 輸出載入的項目數量
            GenerateUI();
            }
            catch (JsonException ex)
            {
                Debug.LogError("JSON parsing error: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError($"Failed to load data: {request.error}");
            // 如果網路失敗，就讀本地檔案
            LoadOrCreateItemData();
            GenerateUI();
        }
    }

    void LoadOrCreateItemData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log($"Loaded JSON from File: {json}");
            itemDataList = JsonConvert.DeserializeObject<ItemDataList>(json);
        }
        else
        {
            itemDataList = new ItemDataList { Items = new List<ItemData>() };
            itemDataList.Items.Add(new ItemData { Name = "生命藥水", Price = 100, Description = "恢復生命", Quantity = 0, SpriteName = "potion" });
            itemDataList.Items.Add(new ItemData { Name = "魔法卷軸", Price = 250, Description = "學習新技能", Quantity = 0, SpriteName = "scroll" });
            SaveItemData();
        }
    }

    void SaveItemData()
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonConvert.SerializeObject(itemDataList, Formatting.Indented);
        Debug.Log($"Saving JSON to File: {json}");
        File.WriteAllText(filePath, json);
    }

    void GenerateUI()
    {
        Debug.Log($"Item Count: {itemDataList.Items.Count}");  // 輸出項目的數量
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in itemDataList.Items)
        {
            Debug.Log($"Creating item: {item.Name}");
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            //Debug.Log(item.Name);
            Debug.Log($"Creating item: {item.Name}");
            newItem.GetComponent<ItemUI>().Setup(item, BuyItem);
        }

        coinText.text = $"金幣: {playerCoins}";
    }

    public void BuyItem(ItemData item)
    {
        if (playerCoins >= item.Price)
        {
            playerCoins -= item.Price;
            item.Quantity++;

            SaveItemData();  // 本地保存（如果需要）
            GenerateUI();
            coinText.text = $"金幣: {playerCoins}";

            // 分別向 Google Sheet 傳送更新資料
            googleSheetManager.UpdateItemToSheet(item);
            googleSheetManager.UpdatePlayerCoins(playerId, playerCoins);
        }
        else
        {
            Debug.Log("金幣不足");
        }
    }
    

    
    public void AddItem(string name, int price, string description, string spriteName)
    {
        itemDataList.Items.Add(new ItemData { Name = name, Price = price, Description = description, Quantity = 0, SpriteName = spriteName });
        SaveItemData();
        GenerateUI();
    }

    public void RemoveItem(string name)
    {
        itemDataList.Items.RemoveAll(item => item.Name == name);
        SaveItemData();
        GenerateUI();
    }

    public void EditItem(string oldName, string newName, int newPrice, string newDescription, string newSpriteName)
    {
        ItemData item = itemDataList.Items.Find(i => i.Name == oldName);
        if (item != null)
        {
            item.Name = newName;
            item.Price = newPrice;
            item.Description = newDescription;
            item.SpriteName = newSpriteName;
            SaveItemData();
            GenerateUI();
        }
    }
    
    
    private IEnumerator PostRequest(string url, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("POST 成功：" + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("POST 錯誤：" + request.error);
        }
    }

}


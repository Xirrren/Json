using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;


public class ShopManager : MonoBehaviour
{
    public Text coinText;
    public GameObject itemPrefab;
    public Transform itemContainer;
    public int playerCoins = 500;
    private string filePath;
    private ItemDataList itemDataList;
    

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "shopData.json");
        LoadOrCreateItemData();
        GenerateUI();
    }

    void LoadOrCreateItemData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log($"Loaded JSON: {json}");  // 確認 JSON 是否有內容
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
        Debug.Log($"Saving JSON: {json}");  // 檢查儲存的內容
        File.WriteAllText(filePath, json);
    }

    void GenerateUI()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var item in itemDataList.Items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            Debug.Log(item.Name);
            newItem.GetComponent<ItemUI>().Setup(item, BuyItem);
        }
    }

    public void BuyItem(ItemData item)
    {
        if (playerCoins >= item.Price)
        {
            playerCoins -= item.Price;
            item.Quantity++;
            SaveItemData();
            GenerateUI();
            coinText.text = $"金幣: {playerCoins}";
        }
    }
    // 新增商品
    public void AddItem(string name, int price, string description, string spriteName)
    {
        itemDataList.Items.Add(new ItemData { Name = name, Price = price, Description = description, Quantity = 0, SpriteName = spriteName });
        SaveItemData();
        GenerateUI();
    }

    // 刪除商品
    public void RemoveItem(string name)
    {
        itemDataList.Items.RemoveAll(item => item.Name == name);
        SaveItemData();
        GenerateUI();
    }

    // 修改商品
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
}

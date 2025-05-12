using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class ItemUI : MonoBehaviour
{
    public Text id;
    public Image itemImage;
    public Text nameText;
    public Text descriptionText;
    public Text priceText;
    public Text quantityText;
    public Button buyButton;
    public SpriteAtlas atlas;
    private ItemData currentItem;
    private System.Action<ItemData> buyCallback;
    
    public void Setup(ItemData item, System.Action<ItemData> buyAction)
    {
        itemImage.sprite = atlas.GetSprite(item.SpriteName);
        currentItem = item;
        buyCallback = buyAction;
        id.text = item.ID;
        nameText.text = item.Name;
        descriptionText.text = item.Description;
        priceText.text = $"價格: {item.Price}";
        quantityText.text = $"數量: {item.Quantity}";
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => buyCallback.Invoke(currentItem));
    }
}
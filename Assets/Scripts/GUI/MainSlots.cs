using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using static UnityEditor.Progress;

public class MainSlots : MonoBehaviour
{
    private GameObject _mainSlots;
    private List<GameObject> _mainSlotItems = new();
    public static int MainSlotsNum = Inventory.SlotNum / 4;
    public static int MaxItemId = EntityCreator.ItemArray.Length;
    private List<Sprite> _itemSprites = new();

    // Start is called before the first frame update
    void Start()
    {
        this._mainSlots = GameObject.Find("ObserverCanvas/MainSlots");

        for (int i = 0; i < MainSlotsNum; i++)
        {
            GameObject newItemImageObject = new($"Slot{i}");
            Image image = newItemImageObject.AddComponent<Image>();
            ClearSlot(image);
            //RectTransform rectTransform = newItemImageObject.GetComponent<RectTransform>();
            //rectTransform.localPosition = new Vector3(rectTransform.sizeDelta.x / 8, 0, 0);
            newItemImageObject.transform.SetParent(_mainSlots.transform);
            newItemImageObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            this._mainSlotItems.Add(newItemImageObject);
        }
        // Initialize Item Sprites
        // Find Texture2D in  and 
        foreach (var itemName in EntityCreator.ItemArray)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Items/{itemName}/{itemName}");
            if (texture == null)
            {
                // Block item
                texture = Resources.Load<Texture2D>($"Blocks/{itemName}/{itemName}");
            }

            if (texture == null)
            {
                _itemSprites.Add(null);
                continue;
            }

            _itemSprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            Debug.Log(_itemSprites.Last());
        }
    }
    private void ClearSlot(Image image)
    {
        image.color = new Color(1, 1, 1, a: 0);
        image.sprite = null;
    }
    private void AddSlotItem(Image image, int itemId)
    {
        if (itemId < 0 || itemId > MaxItemId) return;

        image.color = new Color(1, 1, 1, 1);
        image.sprite = this._itemSprites[itemId];
    }
}

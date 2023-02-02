using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    /// <summary>
    /// The item is smaller than the block, so (item scale) * _itemScaleRate = (block scale)
    /// </summary>
    private float _itemScaleRate = 2f;
    /// <summary>
    /// The continuous item id is defined by ourselves, rather than original edition of MC
    /// Owing to the item also contains the block, we list the block at first
    /// </summary>
    public static string[] ItemArray =
    {
        "Air",
        "Stone",
        "Grass",
        "Dirt",
        "Cobblestone",
        "Planks",
        "Bedrock",
        "Log",
        "Apple",
        "WoodenSword",
        "WoodenShovel",
        "WoodenPickaxe",
        "WoodenAxe",
        "StoneSword",
        "StoneShovel",
        "StonePickaxe",
        "StoneAxe",
        "Stick"
    };
    public GameObject[] ItemPrefabs;

    // Player
    public static string[] PlayerArray =
    {
        "Steve"
    };
    public GameObject[] PlayerPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the "ItemPrefabs"
        ItemPrefabs = new GameObject[ItemArray.Length];

        // Item entity
        for (int i = 0; i < ItemArray.Length; i++)
        {
            string itemName = ItemArray[i];
            //Owing to the item also contains the block, find if the blockDictionary contains this itemName at first
            if (BlockCreator.BlockPrefabDict.ContainsKey(itemName))
            {
                ItemPrefabs[i] = Resources.Load<GameObject>($"Blocks/{itemName}/{itemName}");
            }
            else
            {
                ItemPrefabs[i] = Resources.Load<GameObject>($"Items/{itemName}/{itemName}");
            }
        }
        // Player entity
        PlayerPrefabs = new GameObject[PlayerArray.Length];

        for (int i = 0; i < PlayerArray.Length; i++)
        {
            string playerName = PlayerArray[i];
            PlayerPrefabs[i] = Resources.Load<GameObject>($"Player/{playerName}/{playerName}");
        }
    }
    /// <summary>
    /// Create a item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CreateItem(Item item)
    {
        // Create the item if the block is not air and in the bounds of ItemArray
        if (item.Id <= 0 || item.Id >= ItemArray.Length)
            return false;

        // Create the item if the block hasn't been created
        if (item.EntityObject != null)
            return false;

        GameObject itemObject;
        // Miss texture?

        // Instantiate 
        itemObject = (GameObject)Instantiate(ItemPrefabs[item.Id]);

        item.EntityObject = itemObject;
        // Put the object in a right position, its parent is 'ItemCreator' object
        itemObject.transform.parent = this.transform;
        itemObject.transform.position = item.Position;
        // Scale
        itemObject.transform.localScale /= this._itemScaleRate;
        // Add Interpolate Movement
        itemObject.AddComponent<InterpolateMovement>();
        // Add item
        EntitySource.AddItem(item);

        // Create the item successfully
        return true;
    }
    /// <summary>
    /// Delete a item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool DeleteItem(Item item)
    {
        // False if the item not exist
        if (item == null) return false;

        // Check if the item obj exists
        if (item.EntityObject != null)
        {
            Destroy(item.EntityObject);
        }
        // Delete the item from dict
        if (EntitySource.ItemDict.ContainsKey(item.UniqueId))
        {
            EntitySource.ItemDict.Remove(item.UniqueId);
        }
        // Delete the item successfully
        return true;
    }
    public bool CreatePlayer(Player player)
    {
        return true;
        // Add Interpolate Movement
        //playerObject.AddComponent<InterpolateMovement>();
    }
}

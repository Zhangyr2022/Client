using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCreator : MonoBehaviour
{
    /// <summary>
    /// The item is smaller than the block, so (item scale) * _itemScaleRate = (block scale)
    /// </summary>
    private float _itemScaleRate;
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
    /// <summary>
    /// The allItemDict store all the items in the game 
    /// </summary>
    public Dictionary<int, GameObject> allItemDict;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize the "ItemPrefabs"
        ItemPrefabs = new GameObject[ItemArray.Length];

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
        if (item.EntityObject != null || allItemDict.ContainsKey(item.UniqueId))
            return false;

        GameObject itemObject;
        // Miss texture?

        // Instantiate 
        itemObject = (GameObject)Instantiate(ItemPrefabs[item.Id]);

        item.EntityObject = itemObject;
        // Put the object in a right position, its parent is 'ItemCreator' object
        itemObject.transform.parent = this.transform;
        // The center of block is (0.5+x, 0.5+y, 0.5+z)
        itemObject.transform.position = item.Position;

        // Add the obj into the allItemDict
        allItemDict.Add(item.UniqueId, itemObject);

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
        // Delete the item from dict

        // Delete the item successfully
        return true;
    }
}

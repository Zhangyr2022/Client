using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class EntitySource
{
    /// <summary>
    /// The allItemDict store all the items in the game 
    /// </summary>
    public static Dictionary<int, Item> ItemDict = new();
    public static bool AddItem(Item item)
    {
        if (ItemDict.ContainsKey(item.UniqueId))
            return false;

        if (item.EntityObject == null)
            return false;

        ItemDict.Add(item.UniqueId, item);
        return true;
    }
    public static Item GetItem(int uniqueId)
    {
        if (ItemDict.ContainsKey(uniqueId))
        {
            return ItemDict[uniqueId];
        }
        else
        {
            return null;
        }
    }
}

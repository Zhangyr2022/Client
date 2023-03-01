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
    /// Open its renderer
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool SpawnEntity(Entity entity)
    {
        entity.EntityObject.TryGetComponent(out Renderer renderer);
        if (renderer == null)
            return false;

        renderer.enabled = true;
        return true;
    }
    /// <summary>
    /// Close its renderer
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool DespawnEntity(Entity entity, int entityTypeId)
    {
        entity.EntityObject.TryGetComponent(out Renderer renderer);
        if (renderer == null)
            return false;

        if (entityTypeId == 0)
        {
            ((Player)entity).PlayerAnimations.DeadAnimationPlayer();

            // Not rendered when the dead animations end
            void SetNotRendered()
            {
                renderer.enabled = false;
            }
            Invoke(nameof(SetNotRendered), PlayerAnimations.DeadTime);
        }
        else if (entityTypeId == 1)
        {
            renderer.enabled = false;
        }
        return true;
    }
    /// <summary>
    /// Create a item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CreateItem(Item item)
    {
        // Create the item if the block is not air and in the bounds of ItemArray
        if (item.Id < 0 || item.Id >= ItemArray.Length)
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
        itemObject.transform.Rotate(new Vector3(item.pitch, item.yaw, 0));
        // Scale
        itemObject.transform.localScale /= this._itemScaleRate;
        // Add Interpolate Movement
        itemObject.AddComponent<InterpolateMovement>();
        itemObject.AddComponent<ItemRotation>();
        // Add item
        EntitySource.AddItem(item);

        // Create the item successfully
        return true;
    }
    private bool DeleteItemObject(Item item)
    {
        // False if the item not exist
        if (item == null) return false;

        // Check if the item obj exists
        if (item.EntityObject != null)
        {
            Destroy(item.EntityObject);
        }

        // Delete the item successfully
        return true;
    }
    private bool DeleteItemFromDict(Item item)
    {
        // Delete the item from dict
        if (EntitySource.ItemDict.ContainsKey(item.UniqueId))
            return false;

        EntitySource.ItemDict.Remove(item.UniqueId);
        return true;
    }
    /// <summary>
    /// Delete a item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool DeleteItem(Item item)
    {
        bool deletObject = DeleteItemObject(item);
        bool deleteFromDict = DeleteItemFromDict(item);
        if (deletObject && deleteFromDict)
            return true;
        return false;
    }
    public bool CreatePlayer(Player player)
    {
        // Create the item if the block hasn't been created
        if (player.EntityObject != null)
            return false;

        GameObject playerObject;
        // Miss texture?

        // Instantiate 
        playerObject = (GameObject)Instantiate(PlayerPrefabs[player.Id]);

        player.EntityObject = playerObject;
        // Put the object in a right position, its parent is 'ItemCreator' object
        playerObject.transform.parent = this.transform;
        playerObject.transform.position = player.Position;

        // Add Interpolate Movement
        playerObject.AddComponent<InterpolateMovement>();
        // Add player
        EntitySource.AddPlayer(player);

        // Add Interpolate Movement
        player.InterpolateMove = player.EntityObject.AddComponent<InterpolateMovement>();
        // Add Animation players
        player.PlayerAnimations = player.EntityObject.AddComponent<PlayerAnimations>();

        // Update Body components: head, arms, legs
        player.UpdateBodyGameObject();
        player.UpdateOrientation(player.pitch, player.yaw);

        return true;
    }
    private bool DeletePlayerObject(Player player)
    {
        // False if the player not exist
        if (player == null) return false;

        // Check if the player obj exists
        if (player.EntityObject != null)
        {
            Destroy(player.EntityObject);
        }

        // Delete the player successfully
        return true;
    }
    private bool DeletePlayerFromDict(Player player)
    {
        // Delete the player from dict
        if (!EntitySource.PlayerDict.ContainsKey(player.UniqueId))
            return false;

        EntitySource.PlayerDict.Remove(player.UniqueId);
        return true;
    }
    public bool DeletePlayer(Player player)
    {
        bool deletObject = DeletePlayerObject(player);
        bool deleteFromDict = DeletePlayerFromDict(player);
        if (deletObject && deleteFromDict)
            return true;
        return false;
    }
    public void DeleteAllEntities()
    {
        foreach (Player player in EntitySource.PlayerDict.Values)
        {
            DeletePlayerObject(player);
        }
        EntitySource.PlayerDict.Clear();

        foreach (Item item in EntitySource.ItemDict.Values)
        {
            DeleteItemObject(item);
        }
        EntitySource.ItemDict.Clear();
    }
}

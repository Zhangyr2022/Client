using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCreator : MonoBehaviour
{
    public Dictionary<string, int> BlockPrefabDict = new Dictionary<string, int>{
        {"Air",0},
        {"Stone",1 },
        {"Grass",2 },
        {"Dirt",3 },
        {"Planks",4 },
        {"Bedrock",5 },
        {"Log",6 },
        {"Leaves",7 }
    };
    public Object[] BlockPrefabs;  // Find in the all prefabs

    // Start is called before the first frame update
    void Start()
    {
        // Load all the Block prefabs
        BlockPrefabs = new Object[this.BlockPrefabDict.Count];
        foreach (var prefabInfo in this.BlockPrefabDict)
        {
            string name = prefabInfo.Key;
            int index = prefabInfo.Value;
            BlockPrefabs[index] = Resources.Load<Object>($"Blocks/{name}/{name}");
        }
    }
    /// <summary>
    /// Create a block in the unity (make the block become a GameObject)
    /// </summary>
    /// <param name="block">The block to be created in the Unity</param>
    /// <returns></returns>
    public bool CreateBlock(Block block)
    {
        // Create the block if the block hasn't been created
        if (block.BlockObject != null)
            return false;
        // Create the block if the block is not air
        if (block.Id == 0)
            return false;

        // Get the index in array "BlockPrefabs"
        int prefabIndex = BlockPrefabDict["Stone"]; // If the cube cannot be found, create stone
        if (BlockPrefabDict.ContainsKey(block.Name))
        {
            prefabIndex = BlockPrefabDict[block.Name];
        }

        // Create block object
        block.BlockObject = (GameObject)Instantiate(BlockPrefabs[prefabIndex]);
        // Put the object in a right position, its parent is 'BlockCreator' object
        block.BlockObject.transform.parent = this.transform;
        // The center of block is (0.5+x, 0.5+y, 0.5+z)
        block.BlockObject.transform.position = new Vector3(block.Position.x + 0.5f, block.Position.y + 0.5f, block.Position.z + 0.5f);
        return true;
    }
}

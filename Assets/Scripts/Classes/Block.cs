using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public short Id;
    public string Name;
    public Vector3Int Position;// The absolute position in the map
    public GameObject BlockObject; // If the block is not created, this variable will be null
    public Block(short id, Vector3Int position, GameObject blockObject = null)
    {
        this.Id = id;
        this.Name = BlockDicts.BlockNameArray[id];
        this.Position = position;
        this.BlockObject = blockObject;
    }
}

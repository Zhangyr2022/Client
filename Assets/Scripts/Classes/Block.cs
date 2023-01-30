using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public short Id;
    public string Name;
    public Vector3Int Position;// The absolute position in the map
    public GameObject BlockObject; // If the block is not created, this variable will be null
}

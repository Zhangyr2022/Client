using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Entity
{
    public int UniqueId;
    public Vector3 Position;
    public GameObject EntityObject;
    public InterpolateMovement InterpolateMove;
}

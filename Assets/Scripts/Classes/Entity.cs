using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public enum Event
    {
        None,
        Spawn,
        Despawn
    }
    public int UniqueId;
    public int EntityId;
    public int DataValue;
    public Vector3 Position;
    public GameObject EntityObject;
    public int yaw;
    public int pitch;
    public InterpolateMovement InterpolateMove;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Entity
{
    /// <summary>
    /// Different from this._uniqueId, the Id is the type of this item
    /// </summary>
    public int Id;
    public Item(int uniqueId, Vector3 position, int id)
    {
        this.UniqueId = uniqueId;
        this.Position = position;
        this.Id = id;
        this.EntityObject = null;
    }
    /// <summary>
    /// Change the position of this item
    /// </summary>
    /// <param name="newPosition"></param>
    public void UpdatePosition(Vector3 newPosition)
    {
        this.Position = newPosition;
        if (this.EntityObject != null)
        {
            this.EntityObject.transform.position = newPosition;
        }
    }
    public void UpdateOrientation(int pitch, int yaw)
    {
        if (this.EntityObject != null)
        {
            this.EntityObject.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
    }
}

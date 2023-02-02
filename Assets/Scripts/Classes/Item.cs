using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Entity
{
    /// <summary>
    /// Different from this._uniqueId, the Id is the type of this item
    /// </summary>
    public int Id;
    /// <summary>
    /// Owing to the item should move more continuously, so we use interpolation.
    /// This position represant real position of the gameobject
    /// </summary>
    //private Vector3 _nowPosition;
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
            if (this.InterpolateMove != null)
            {
                // Interpolation movement
                this.InterpolateMove.SetTargetPosition(newPosition);
            }
            else
            {
                this.InterpolateMove = this.EntityObject.GetComponent<InterpolateMovement>();
                // No Interpolation movement
                this.EntityObject.transform.position = newPosition;
            }
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

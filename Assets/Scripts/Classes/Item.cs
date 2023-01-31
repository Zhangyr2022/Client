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
}

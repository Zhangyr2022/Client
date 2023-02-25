using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public enum InstantAction
    {
        Jump,
        AttackClick,
        AttackStart,
        AttackEnd,
        UseClick,
        UseStart,
        UseEnd
    }
    public const int SlotNum = 36;
    public float Health;
    public int Experiments;
    public Slot[] Inventory = new Slot[SlotNum];
    public int MainHandSlot;
    public int Id = 0;
    public Player(int uniqueId, Vector3 position, float yaw = 0, float pitch = 0)
    {
        this.UniqueId = uniqueId;
        this.EntityId = 0;
        this.Position = position;
        this.yaw = yaw;
        this.pitch = pitch;
    }
    public Player()
    {
        this.EntityId = 0;
        this.yaw = 0;
        this.pitch = 0;
    }
    public void UpdatePosition(Vector3 newPosition)
    {

    }
}

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
}

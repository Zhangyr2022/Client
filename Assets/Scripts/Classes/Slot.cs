using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Slot
{
    public int SlotIndex;
    public int ItemId;
    public int Count;
    public int Damage;

    public Slot(int slotIndex, int itemId, int count, int damage)
    {
        this.SlotIndex = slotIndex;
        this.ItemId = itemId;
        this.Count = count;
        this.Damage = damage;
    }
    public static void SwapSlot(Slot a, Slot b)
    {
        Slot temp = a;
        a = b;
        b = temp;
    }
}



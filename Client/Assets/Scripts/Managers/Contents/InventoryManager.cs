﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

    public void Add(Item item)
    {
        Items.Add(item.ItemDbId, item);
    }

    public Item Get(int ItemDbId)
    {
        Item item = null;
        Items.TryGetValue(ItemDbId, out item);
        return item;
    }

    public Item Find(Func<Item, bool> condition)
    {
        foreach (var item in Items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }

        return null;
    }

    public void Clear()
    {
        Items.Clear();
    }
}
// Author: 文若
// CreateDate: 2022/10/14

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item 
{
    public int id;
    public int num;

    public Item(int id, int num)
    {
        this.id = id;
        this.num = num;
    }
}

public class BagData
{
    public List<Item> items;

    public BagData()
    {
        items = new List<Item>();
        for (int i = 0; i < 10000; i++)
            items.Add(new Item(i, i));
    }

    public List<Item> GetData()
    {
        return items;
    }
}
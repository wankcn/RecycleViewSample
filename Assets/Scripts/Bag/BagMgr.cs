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

public class BagMgr : BaseManager<BagMgr>
{
    public List<Item> items;

    public void InitData()
    {
        items = new List<Item>();
        for (int i = 0; i < 10000; i++)
            items.Add(new Item(i, i));
    }
}
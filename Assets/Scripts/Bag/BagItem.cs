using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : BasePanel, IItemBase<Item>
{
    public void InitItemInfo(Item item)
    {
        GetControl<Text>("index").text = item.num.ToString();
    }
}
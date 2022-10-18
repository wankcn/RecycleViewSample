// Author: 文若
// CreateDate: 2022/10/14

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour, IItemBase<Item>
{
    public Text index;
    public Image img;
    public void InitItemInfo(Item item)
    {
        index.text = item.num.ToString();
    }
}
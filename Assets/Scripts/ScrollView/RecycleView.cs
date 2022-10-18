// Author: 文若
// CreateDate: 2022/10/14

using System;
using UnityEngine;
public class RecycleView : MonoBehaviour
{
    public RectTransform content;
    public Vector2 itemSize;
    public Vector2 interSpace;
    [Range(1,20)]
    public int showLine;
    public int showViewH;
    private SVCore<Item, BagItem> sv;
    public string boxResPath;
    public GameObject sample;
    private void Start()
    {
        var data = new BagData();
        if (content == null) return;
        sv = new SVCore<Item, BagItem>(sample);
        sv.InitItemResName(boxResPath);
        sv.InitItemSizeAndCol(itemSize, interSpace, showLine);
        sv.InitContentSVH(content, showViewH);
        sv.InitInfos(data.GetData());
    }

    private void Update()
    {
        if (content)
            sv.CheckShowOrHide();
    }

    private void OnDestroy()
    {
        sv.ClearData();
    }
}
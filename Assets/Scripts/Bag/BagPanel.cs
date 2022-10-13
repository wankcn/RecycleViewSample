using UnityEngine;

public class BagPanel : BasePanel
{
    public RectTransform content;
    private CustomSV<Item, BagItem> sv;

    private void Start()
    {
        sv = new CustomSV<Item, BagItem>();
        sv.InitItemResName("UI/BagItem");
        sv.InitItemSizeAndCol(240, 190, 3);
        sv.InitContentSVH(content, 925);
        sv.InitInfos(BagMgr.GetInstance().items);
    }

    private void Update()
    {
        sv.CheckShowOrHide();
    }
}
using UnityEngine;
public class ScrollView : MonoBehaviour
{
    public RectTransform content;
    public Vector2 itemSize;
    public Vector2 interSpace;
    [Range(1,20)]
    public int showLine;
    public int showViewH;
    private CustomSV<Item, BagItem> sv;
    public string boxResPath;
    public GameObject sample;
    private void Start()
    {
        var data = new BagData();
        if (content == null) return;
        sv = new CustomSV<Item, BagItem>();
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
}
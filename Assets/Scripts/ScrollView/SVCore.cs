// Author: 文若
// CreateDate: 2022/10/14

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// item对象必须继承，用于实现初始化item方法
/// </summary>
public interface IItemBase<T>
{
    void InitItemInfo(T data);
}

/// <summary>
/// 自定义SV
/// </summary>
/// <typeparam name="T">数据来源</typeparam>
/// <typeparam name="K">Item类,需要继承IItemBase</typeparam>
public class SVCore<T, K> where K : IItemBase<T>
{
    private RectTransform content;
    private int viewRangeH; // 可视化范围的高
    private Dictionary<int, GameObject> nowShowItems;
    private List<T> items; // 数据来源
    private int oldMinIndex = -1;
    private int oldMaxIndex = -1;
    private int itemW; // 间隔宽
    private int itemH; // 间隔高
    private int col; // 列数
    private string itemResName; // item预设路径
    private SVPool svPool; 

    public SVCore(GameObject obj)
    {
        svPool = new SVPool(obj);
    }

    /// <summary>
    /// 初始化预制件资源路径
    /// </summary>
    /// <param name="path">滑动对象的资源路径（不包含后缀名）</param>
    public void InitItemResName(string path)
    {
        itemResName = path;
    }

    /// <summary>
    /// 初始化数据，并且设置content的高
    /// </summary>
    public void InitInfos(List<T> data)
    {
        this.items = data;
        nowShowItems = new Dictionary<int, GameObject>();
        content.sizeDelta = new Vector2(0, Mathf.CeilToInt(items.Count / col) * itemH);
    }

    /// <summary>
    /// 初始化父对象以及可视化范围
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="h"></param>
    public void InitContentSVH(RectTransform transform, int h)
    {
        this.content = transform;
        this.viewRangeH = h;
    }

    /// <summary>
    /// 初始化格子布局
    /// </summary>
    /// <param name="w">间隔宽度</param>
    /// <param name="h">间隔高度</param>
    /// <param name="col">显示列数</param>
    public void InitItemSizeAndCol(int w, int h, int col)
    {
        this.itemW = w;
        this.itemH = h;
        this.col = col;
    }

    /// <summary>
    /// 初始化布局
    /// </summary>
    /// <param name="item">Vector 格子长宽</param>
    /// <param name="interSpaceX">横行间隔</param>
    /// <param name="interSpaceY">纵向间隔</param>
    public void InitItemSizeAndCol(Vector2 item, int interSpaceX, int interSpaceY, int col)
    {
        this.itemW = (int) item.x + interSpaceX;
        this.itemH = (int) item.y + interSpaceY;
        this.col = col;
    }

    public void InitItemSizeAndCol(Vector2 item, Vector2 interSpace, int col)
    {
        this.itemW = (int) (item.x + interSpace.x);
        this.itemH = (int) (item.y + interSpace.y);
        this.col = col;
    }

    /// <summary>
    /// 显示格子布局
    /// </summary>
    public void CheckShowOrHide()
    {
        int minIndex = (int) (content.anchoredPosition.y / itemH) * col;
        int maxIndex = (int) ((content.anchoredPosition.y + viewRangeH) / itemH) * col + col - 1;

        // 不能超出最大值和小于最小值
        if (minIndex < 0)
            minIndex = 0;
        if (maxIndex > items.Count)
            maxIndex = items.Count - 1;

        // 当前索引值和上一次索引值不同时在进行更新节省开销
        if (minIndex != oldMinIndex || maxIndex != oldMaxIndex)
        {
            // 根据上一次索引和新索引 删减prefab
            for (int i = oldMinIndex; i < minIndex; i++)
            {
                if (nowShowItems.ContainsKey(i))
                {
                    // 需要判空，可能还没加载出来
                    if (nowShowItems[i] != null)
                    {
                        svPool.PushObj(itemResName, nowShowItems[i]);
                    }

                    nowShowItems.Remove(i);
                }
            }

            // 删除下部溢出
            for (int i = oldMaxIndex + 1; i < maxIndex; i++)
            {
                if (nowShowItems.ContainsKey(i))
                {
                    // 需要判空，可能还没加载出来
                    if (nowShowItems[i] != null)
                    {
                        svPool.PushObj(itemResName, nowShowItems[i]);
                    }

                    nowShowItems.Remove(i);
                }
            }
        }

        oldMinIndex = minIndex;
        oldMaxIndex = maxIndex;

        // 创建指定范围内格子数
        for (int i = minIndex; i <= maxIndex; i++)
        {
            if (nowShowItems.ContainsKey(i))
            {
                continue;
            }
            else
            {
                int index = i;
                // 异步加载，当前帧可能创建不出来，现在外面站坑
                nowShowItems.Add(index, null);
                svPool.GetObj(itemResName, (obj) =>
                {
                    // 设置父对象
                    obj.transform.SetParent(content.transform, false);
                    // 重置相对缩放大小
                    obj.transform.localScale = Vector3.one;
                    // 重置位置
                    obj.transform.localPosition = new Vector3(index % col * itemW, -index / col * itemH, 0);
                    // 更新格子信息
                    obj.GetComponent<K>().InitItemInfo(items[index]);

                    // 判断有没有坑位
                    if (nowShowItems.ContainsKey(index))
                    {
                        nowShowItems[index] = obj;
                    }
                    else
                    {
                        // 如果操作太快，这个obj就不要了
                        svPool.PushObj(itemResName, obj);
                    }
                });
            }
        }
    }
    
    public void ClearData(){
        svPool.Clear();
    }

}
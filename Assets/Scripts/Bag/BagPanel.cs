using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagPanel : BasePanel
{
    public RectTransform content;

    public int viewPiontH;

    public Dictionary<int, GameObject> nowShowItems = new Dictionary<int, GameObject>();

    private int oldMinIndex = -1;
    private int oldMaxIndex = -1;

    private void Update()
    {
        CheckShowOrHide();
    }


    public override void ShowMe()
    {
        base.ShowMe();
        content.sizeDelta = new Vector2(0, Mathf.CeilToInt(BagMgr.GetInstance().items.Count / 3f) * 190);
        // CheckShowOrHide();
    }

    /// <summary>
    /// 显示格子
    /// </summary>
    void CheckShowOrHide()
    {
        // 如果坐标小于0不处理
        if (content.anchoredPosition.y < 0) return;
        int minIndex = (int) (content.anchoredPosition.y / 190) * 3;
        int maxIndex = (int) ((content.anchoredPosition.y + viewPiontH) / 190) * 3 + 2;

        // 不能超出最大值
        if (maxIndex > BagMgr.GetInstance().items.Count)
            maxIndex = BagMgr.GetInstance().items.Count - 1;

        // 根据上一次索引和新索引 删减prefab
        for (int i = oldMinIndex; i < minIndex; i++)
        {
            if (nowShowItems.ContainsKey(i))
            {
                // 需要判空，可能还没加载出来
                if (nowShowItems[i] != null)
                {
                    PoolMgr.GetInstance().PushObj("UI/BagItem", nowShowItems[i]);
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
                    PoolMgr.GetInstance().PushObj("UI/BagItem", nowShowItems[i]);
                }

                nowShowItems.Remove(i);
            }
        }

        oldMinIndex = minIndex;
        oldMaxIndex = maxIndex;


        // 创建指定范围内格子数
        for (int i = minIndex; i <= maxIndex; i++)
        {
            // 
            if (nowShowItems.ContainsKey(i))
            {
                continue;
            }
            else
            {
                int index = i;
                // 异步加载，当前帧可能创建不出来，现在外面站坑
                nowShowItems.Add(index, null);
                PoolMgr.GetInstance().GetObj("UI/BagItem", (obj) =>
                {
                    // 设置父对象
                    obj.transform.SetParent(content.transform, false);
                    // 重置相对缩放大小
                    obj.transform.localScale = Vector3.one;
                    // 重置位置
                    obj.transform.localPosition = new Vector3(index % 3 * 240, -index / 3 * 190, 0);
                    // 更新格子信息
                    obj.GetComponent<BagItem>().InitItemInfo(BagMgr.GetInstance().items[index]);

                    // 判断有没有坑位
                    if (nowShowItems.ContainsKey(index))
                    {
                        nowShowItems[index] = obj;
                    }
                    else
                    {
                        // 如果操作太快，这个obj就不要了
                        PoolMgr.GetInstance().PushObj("UI/BagItem", obj);
                    }
                });
            }
        }
    }
}
// Author: 文若
// CreateDate: 2022/10/26

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Serialization;

namespace GameNeon.Modules.UIModule
{
    public enum e_Direction
    {
        Horizontal,
        Vertical
    }


    public class RecycleView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public GameObject m_PointingFirstArrow;
        public GameObject m_PointingEndArrow;
        public e_Direction dir = e_Direction.Vertical;
        public bool m_IsShowArrow = false;

        public int lines = 1; // 默认显示1行
        public float squareSpacing = 5f; // 方阵间距
        public GameObject cell; //指定的cell
        public Vector2 Spacing = Vector2.zero;
        public float row = 0f; // 行间距
        public float col = 0f; // 列间距

        protected Action<GameObject, int> m_FuncCallBackFunc;
        protected Action<GameObject, int> m_FuncOnClickCallBack;
        protected Action<int, bool, GameObject> m_FuncOnButtonClickCallBack;

        protected RectTransform rectTrans;

        protected float planeW;
        protected float planeH;

        protected float contentW;
        protected float contentH;

        protected float cellW;
        protected float cellH;

        protected GameObject content;
        protected RectTransform contentRectTrans;

        private bool m_isInited = false;

        //记录 物体的坐标 和 物体 
        protected struct CellInfo
        {
            public Vector3 pos;
            public GameObject obj;
        };

        protected CellInfo[] cellInfos;

        protected bool isInited = false;

        protected ScrollRect scrollRect;

        protected int maxCount = -1; //列表数量

        protected int minIndex = -1;
        protected int maxIndex = -1;

        protected bool isClearList = false; //是否清空列表

        public virtual void Init(Action<GameObject, int> callBack)
        {
            Init(callBack, null);
        }

        public virtual void Init(Action<GameObject, int> callBack, Action<GameObject, int> onClickCallBack,
            Action<int, bool, GameObject> onButtonClickCallBack)
        {
            if (onButtonClickCallBack != null)
            {
                m_FuncOnButtonClickCallBack = onButtonClickCallBack;
            }

            Init(callBack, onClickCallBack);
        }

        public virtual void Init(Action<GameObject, int> callBack, Action<GameObject, int> onClickCallBack)
        {
            DisposeAll();

            m_FuncCallBackFunc = callBack;

            if (onClickCallBack != null)
            {
                m_FuncOnClickCallBack = onClickCallBack;
            }

            if (m_isInited)
                return;


            content = this.GetComponent<ScrollRect>().content.gameObject;

            if (cell == null)
            {
                cell = content.transform.GetChild(0).gameObject;
            }

            /* Cell 处理 */
            //m_CellGameObject.transform.SetParent(m_Content.transform.parent, false);
            SetPoolsObj(cell);

            RectTransform cellRectTrans = cell.GetComponent<RectTransform>();
            cellRectTrans.pivot = new Vector2(0f, 1f);
            CheckAnchor(cellRectTrans);
            cellRectTrans.anchoredPosition = Vector2.zero;

            //记录 Cell 信息
            cellH = cellRectTrans.rect.height;
            cellW = cellRectTrans.rect.width;

            //记录 Plane 信息
            rectTrans = GetComponent<RectTransform>();
            Rect planeRect = rectTrans.rect;
            planeH = planeRect.height;
            planeW = planeRect.width;

            //记录 Content 信息
            contentRectTrans = content.GetComponent<RectTransform>();
            Rect contentRect = contentRectTrans.rect;
            contentH = contentRect.height;
            contentW = contentRect.width;

            // 记录间距信息
            row = Spacing.x;
            col = Spacing.y;

            contentRectTrans.pivot = new Vector2(0f, 1f);
            //m_ContentRectTrans.sizeDelta = new Vector2 (planeRect.width, planeRect.height);
            //m_ContentRectTrans.anchoredPosition = Vector2.zero;
            
            CheckAnchor(contentRectTrans);
            scrollRect = this.GetComponent<ScrollRect>();
            scrollRect.onValueChanged.RemoveAllListeners();
            
            //添加滑动事件
            scrollRect.onValueChanged.AddListener(delegate(Vector2 value) { ScrollRectListener(value); });
            if (m_PointingFirstArrow != null || m_PointingEndArrow != null)
            {
                scrollRect.onValueChanged.AddListener(delegate(Vector2 value) { OnDragListener(value); });
                OnDragListener(Vector2.zero);
            }

            //InitScrollBarGameObject(); // 废弃

            m_isInited = true;
        }

        //检查 Anchor 是否正确
        private void CheckAnchor(RectTransform rectTrans)
        {
            if (dir == e_Direction.Vertical)
            {
                if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1)) ||
                      (rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(1, 1))))
                {
                    rectTrans.anchorMin = new Vector2(0, 1);
                    rectTrans.anchorMax = new Vector2(1, 1);
                }
            }
            else
            {
                if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1)) ||
                      (rectTrans.anchorMin == new Vector2(0, 0) && rectTrans.anchorMax == new Vector2(0, 1))))
                {
                    rectTrans.anchorMin = new Vector2(0, 0);
                    rectTrans.anchorMax = new Vector2(0, 1);
                }
            }
        }

        //实时刷新列表时用
        public virtual void UpdateList()
        {
            for (int i = 0, length = cellInfos.Length; i < length; i++)
            {
                CellInfo cellInfo = cellInfos[i];
                if (cellInfo.obj != null)
                {
                    float rangePos = dir == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                    if (!IsOutRange(rangePos))
                    {
                        Func(m_FuncCallBackFunc, cellInfo.obj, true);
                    }
                }
            }
        }

        //刷新某一项
        public void UpdateCell(int index)
        {
            CellInfo cellInfo = cellInfos[index - 1];
            if (cellInfo.obj != null)
            {
                float rangePos = dir == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                if (!IsOutRange(rangePos))
                {
                    Func(m_FuncCallBackFunc, cellInfo.obj);
                }
            }
        }

        public virtual void ShowList(string numStr)
        {
        }

        public virtual void ShowList(int num)
        {
            minIndex = -1;
            maxIndex = -1;

            //-> 计算 Content 尺寸
            if (dir == e_Direction.Vertical)
            {
                float contentSize = (squareSpacing + cellH) * Mathf.CeilToInt((float)num / lines);
                contentH = contentSize;
                contentW = contentRectTrans.sizeDelta.x;
                contentSize = contentSize < rectTrans.rect.height ? rectTrans.rect.height : contentSize;
                contentRectTrans.sizeDelta = new Vector2(contentW, contentSize);
                if (num != maxCount)
                {
                    contentRectTrans.anchoredPosition = new Vector2(contentRectTrans.anchoredPosition.x, 0);
                }
            }
            else
            {
                float contentSize = (squareSpacing + cellW) * Mathf.CeilToInt((float)num / lines);
                contentW = contentSize;
                contentH = contentRectTrans.sizeDelta.x;
                contentSize = contentSize < rectTrans.rect.width ? rectTrans.rect.width : contentSize;
                contentRectTrans.sizeDelta = new Vector2(contentSize, contentH);
                if (num != maxCount)
                {
                    contentRectTrans.anchoredPosition = new Vector2(0, contentRectTrans.anchoredPosition.y);
                }
            }

            //-> 计算 开始索引
            int lastEndIndex = 0;

            //-> 过多的物体 扔到对象池 ( 首次调 ShowList函数时 则无效 )
            if (isInited)
            {
                lastEndIndex = num - maxCount > 0 ? maxCount : num;
                lastEndIndex = isClearList ? 0 : lastEndIndex;

                int count = isClearList ? cellInfos.Length : maxCount;
                for (int i = lastEndIndex; i < count; i++)
                {
                    if (cellInfos[i].obj != null)
                    {
                        SetPoolsObj(cellInfos[i].obj);
                        cellInfos[i].obj = null;
                    }
                }
            }

            //-> 以下四行代码 在for循环所用
            CellInfo[] tempCellInfos = cellInfos;
            cellInfos = new CellInfo[num];

            //-> 1: 计算 每个Cell坐标并存储 2: 显示范围内的 Cell
            for (int i = 0; i < num; i++)
            {
                // * -> 存储 已有的数据 ( 首次调 ShowList函数时 则无效 )
                if (maxCount != -1 && i < lastEndIndex)
                {
                    CellInfo tempCellInfo = tempCellInfos[i];
                    //-> 计算是否超出范围
                    float rPos = dir == e_Direction.Vertical ? tempCellInfo.pos.y : tempCellInfo.pos.x;
                    if (!IsOutRange(rPos))
                    {
                        //-> 记录显示范围中的 首位index 和 末尾index
                        minIndex = minIndex == -1 ? i : minIndex; //首位index
                        maxIndex = i; // 末尾index

                        if (tempCellInfo.obj == null)
                        {
                            tempCellInfo.obj = GetPoolsObj();
                        }

                        tempCellInfo.obj.transform.GetComponent<RectTransform>().anchoredPosition = tempCellInfo.pos;
                        tempCellInfo.obj.name = i.ToString();
                        tempCellInfo.obj.SetActive(true);

                        Func(m_FuncCallBackFunc, tempCellInfo.obj);
                    }
                    else
                    {
                        SetPoolsObj(tempCellInfo.obj);
                        tempCellInfo.obj = null;
                    }

                    cellInfos[i] = tempCellInfo;
                    continue;
                }

                CellInfo cellInfo = new CellInfo();

                float pos = 0; //坐标( isVertical ? 记录Y : 记录X )
                float rowPos = 0; //计算每排里面的cell 坐标

                // * -> 计算每个Cell坐标
                if (dir == e_Direction.Vertical)
                {
                    pos = cellH * Mathf.FloorToInt(i / lines) +
                          squareSpacing * Mathf.FloorToInt(i / lines);
                    rowPos = cellW * (i % lines) + squareSpacing * (i % lines);
                    cellInfo.pos = new Vector3(rowPos, -pos, 0);
                }
                else
                {
                    pos = cellW * Mathf.FloorToInt(i / lines) + squareSpacing * Mathf.FloorToInt(i / lines);
                    rowPos = cellH * (i % lines) + squareSpacing * (i % lines);
                    cellInfo.pos = new Vector3(pos, -rowPos, 0);
                }

                //-> 计算是否超出范围
                float cellPos = dir == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                if (IsOutRange(cellPos))
                {
                    cellInfo.obj = null;
                    cellInfos[i] = cellInfo;
                    continue;
                }

                //-> 记录显示范围中的 首位index 和 末尾index
                minIndex = minIndex == -1 ? i : minIndex; //首位index
                maxIndex = i; // 末尾index

                //-> 取或创建 Cell
                GameObject cell = GetPoolsObj();
                cell.transform.GetComponent<RectTransform>().anchoredPosition = cellInfo.pos;
                cell.gameObject.name = i.ToString();

                //-> 存数据
                cellInfo.obj = cell;
                cellInfos[i] = cellInfo;

                //-> 回调  函数
                Func(m_FuncCallBackFunc, cell);
            }

            maxCount = num;
            isInited = true;

            OnDragListener(Vector2.zero);
        }

        // 更新滚动区域的大小
        public void UpdateSize()
        {
            Rect rect = GetComponent<RectTransform>().rect;
            planeH = rect.height;
            planeW = rect.width;
        }

        //滑动事件
        protected virtual void ScrollRectListener(Vector2 value)
        {
            UpdateCheck();
        }

        private void UpdateCheck()
        {
            if (cellInfos == null)
                return;

            //检查超出范围
            for (int i = 0, length = cellInfos.Length; i < length; i++)
            {
                CellInfo cellInfo = cellInfos[i];
                GameObject obj = cellInfo.obj;
                Vector3 pos = cellInfo.pos;

                float rangePos = dir == e_Direction.Vertical ? pos.y : pos.x;
                //判断是否超出显示范围
                if (IsOutRange(rangePos))
                {
                    //把超出范围的cell 扔进 poolsObj里
                    if (obj != null)
                    {
                        SetPoolsObj(obj);
                        cellInfos[i].obj = null;
                    }
                }
                else
                {
                    if (obj == null)
                    {
                        //优先从 poolsObj中 取出 （poolsObj为空则返回 实例化的cell）
                        GameObject cell = GetPoolsObj();
                        cell.transform.localPosition = pos;
                        cell.gameObject.name = i.ToString();
                        cellInfos[i].obj = cell;

                        Func(m_FuncCallBackFunc, cell);
                    }
                }
            }
        }

        //判断是否超出显示范围
        protected bool IsOutRange(float pos)
        {
            Vector3 listP = contentRectTrans.anchoredPosition;
            if (dir == e_Direction.Vertical)
            {
                if (pos + listP.y > cellH || pos + listP.y < -rectTrans.rect.height)
                {
                    return true;
                }
            }
            else
            {
                if (pos + listP.x < -cellW || pos + listP.x > rectTrans.rect.width)
                {
                    return true;
                }
            }

            return false;
        }

        //对象池 机制  (存入， 取出) cell
        protected Stack<GameObject> poolsObj = new Stack<GameObject>();

        //取出 cell
        protected virtual GameObject GetPoolsObj()
        {
            GameObject cell = null;
            if (poolsObj.Count > 0)
            {
                cell = poolsObj.Pop();
            }

            if (cell == null)
            {
                cell = Instantiate(this.cell) as GameObject;
            }

            cell.transform.SetParent(content.transform);
            cell.transform.localScale = Vector3.one;
            SetActive(cell, true);

            return cell;
        }

        //存入 cell
        protected virtual void SetPoolsObj(GameObject cell)
        {
            if (cell != null)
            {
                poolsObj.Push(cell);
                SetActive(cell, false);
            }
        }

        //回调
        protected void Func(Action<GameObject, int> func, GameObject selectObject, bool isUpdate = false)
        {
            int num = int.Parse(selectObject.name) + 1;
            if (func != null)
            {
                func(selectObject, num);
            }
        }

        public void DisposeAll()
        {
            if (m_FuncCallBackFunc != null)
            {
                m_FuncCallBackFunc = null;
            }

            if (m_FuncOnClickCallBack != null)
            {
                m_FuncOnClickCallBack = null;
            }
        }

        protected void OnDestroy()
        {
            DisposeAll();
        }

        public virtual void OnClickCell(GameObject cell)
        {
        }

        //-> ExpandCircularScrollView 函数
        public virtual void OnClickExpand(int index)
        {
        }

        //-> FlipCircularScrollView 函数
        public virtual void SetToPageIndex(int index)
        {
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
        }

        protected void OnDragListener(Vector2 value)
        {
            float normalizedPos = dir == e_Direction.Vertical
                ? scrollRect.verticalNormalizedPosition
                : scrollRect.horizontalNormalizedPosition;

            if (dir == e_Direction.Vertical)
            {
                if (contentH - rectTrans.rect.height < 10)
                {
                    SetActive(m_PointingFirstArrow, false);
                    SetActive(m_PointingEndArrow, false);
                    return;
                }
            }
            else
            {
                if (contentW - rectTrans.rect.width < 10)
                {
                    SetActive(m_PointingFirstArrow, false);
                    SetActive(m_PointingEndArrow, false);
                    return;
                }
            }

            if (normalizedPos >= 0.9)
            {
                SetActive(m_PointingFirstArrow, false);
                SetActive(m_PointingEndArrow, true);
            }
            else if (normalizedPos <= 0.1)
            {
                SetActive(m_PointingFirstArrow, true);
                SetActive(m_PointingEndArrow, false);
            }
            else
            {
                SetActive(m_PointingFirstArrow, true);
                SetActive(m_PointingEndArrow, true);
            }
        }

        private static void SetActive(GameObject obj, bool isActive)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }
}
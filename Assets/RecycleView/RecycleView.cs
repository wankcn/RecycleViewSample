// Author: 文若
// CreateDate: 2022/10/26

// ## 生成滑动列表必须以下步骤：
// 1. 持有RecycleView对象rv，rv.Init(callBackFunc)
// 2. 刷新整个列表（首次调用和数量变化时调用）: ShowList(int count)
// 3. 回调: Func(GameObject cell, int index) 
// ----------
// 功能接口看代码，案例详见RecycleViewTest.cs
// 刷新单个项: UpdateCell(int index)
// 刷新列表数据(无数量变化时调用): UpdateList()
// 定位到索引所在当前列表的位置 GoToCellPos(int index)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

namespace WenRuo
{
    public enum E_Direction
    {
        Horizontal,
        Vertical
    }


    public class RecycleView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public GameObject firstArrow;
        public GameObject endArrow;
        public E_Direction dir = E_Direction.Vertical;
        public bool isShowArrow = false;

        public int lines = 1; // 默认显示1行
        public float squareSpacing = 5f; // 方阵间距
        public GameObject cell; //指定的cell
        public Vector2 Spacing = Vector2.zero;
        public float row = 0f; // 行间距
        public float col = 0f; // 列间距
        public float paddingTop = 0f; // 顶部空隙
        public float paddingLeft = 0f; // 左侧空隙


        protected Action<GameObject, int> FuncCallBackFunc;
        protected Action<GameObject, int> FuncOnClickCallBack;
        protected Action<int, bool, GameObject> FuncOnButtonClickCallBack;

        protected float planeW;
        protected float planeH;
        protected float contentW;
        protected float contentH;
        protected float cellW;
        protected float cellH;

        private bool isInit = false;
        protected GameObject content;
        protected ScrollRect scrollRect;
        protected RectTransform rectTrans;
        protected RectTransform contentRectTrans;

        protected int maxCount = -1; //列表数量
        protected int minIndex = -1;
        protected int maxIndex = -1;

        //记录 物体的坐标 和 物体 
        protected struct CellInfo
        {
            public Vector3 pos;
            public GameObject obj;
        };

        protected CellInfo[] cellInfos;
        protected bool isClearList = false; //是否清空列表

        // 对象池
        protected Stack<GameObject> Pool = new Stack<GameObject>();
        protected bool isInited = false;


        public virtual void Init(Action<GameObject, int> callBack)
        {
            Init(callBack, null);
        }

        public virtual void Init(Action<GameObject, int> callBack, Action<GameObject, int> onClickCallBack,
            Action<int, bool, GameObject> onButtonClickCallBack)
        {
            if (onButtonClickCallBack != null)
            {
                FuncOnButtonClickCallBack = onButtonClickCallBack;
            }

            Init(callBack, onClickCallBack);
        }

        public virtual void Init(Action<GameObject, int> callBack, Action<GameObject, int> onClickCallBack)
        {
            DisposeAll();

            FuncCallBackFunc = callBack;

            if (onClickCallBack != null)
            {
                FuncOnClickCallBack = onClickCallBack;
            }

            if (isInit) return;

            content = this.GetComponent<ScrollRect>().content.gameObject;
            if (cell == null)
            {
                cell = content.transform.GetChild(0).gameObject;
            }

            // ////////////////////** Cell 处理 **////////////////////
            // m_CellGameObject.transform.SetParent(m_Content.transform.parent, false);
            SetPoolsObj(cell);

            RectTransform cellRectTrans = cell.GetComponent<RectTransform>();
            cellRectTrans.pivot = new Vector2(0f, 1f);
            CheckAnchor(cellRectTrans);
            cellRectTrans.anchoredPosition = Vector2.zero;

            // 记录 Cell 信息
            cellH = cellRectTrans.rect.height;
            cellW = cellRectTrans.rect.width;

            // 记录 Plane 信息
            rectTrans = GetComponent<RectTransform>();
            Rect planeRect = rectTrans.rect;
            planeH = planeRect.height;
            planeW = planeRect.width;

            // 记录 Content 信息
            contentRectTrans = content.GetComponent<RectTransform>();
            Rect contentRect = contentRectTrans.rect;
            contentH = contentRect.height;
            contentW = contentRect.width;

            // 记录间距信息 如果存在行列设置就引用，没有使用方阵间距
            row = Spacing.x;
            col = Spacing.y;
            if (row == 0 && col == 0) row = col = squareSpacing;
            else squareSpacing = 0;

            contentRectTrans.pivot = new Vector2(0f, 1f);
            //m_ContentRectTrans.sizeDelta = new Vector2 (planeRect.width, planeRect.height);
            //m_ContentRectTrans.anchoredPosition = Vector2.zero;

            CheckAnchor(contentRectTrans);
            scrollRect = this.GetComponent<ScrollRect>();
            scrollRect.onValueChanged.RemoveAllListeners();

            //添加滑动事件
            scrollRect.onValueChanged.AddListener(delegate(Vector2 value) { ScrollRectListener(value); });
            if (firstArrow != null || endArrow != null)
            {
                scrollRect.onValueChanged.AddListener(delegate(Vector2 value) { OnDragListener(value); });
                OnDragListener(Vector2.zero);
            }

            //InitScrollBarGameObject(); // 废弃
            isInit = true;
        }

        // 检查 Anchor 是否正确
        private void CheckAnchor(RectTransform rectTrans)
        {
            if (dir == E_Direction.Vertical)
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

        // 实时刷新列表时用
        public virtual void UpdateList()
        {
            for (int i = 0, length = cellInfos.Length; i < length; i++)
            {
                CellInfo cellInfo = cellInfos[i];
                if (cellInfo.obj != null)
                {
                    float rangePos = dir == E_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                    if (!IsOutRange(rangePos))
                    {
                        Func(FuncCallBackFunc, cellInfo.obj, true);
                    }
                }
            }
        }

        // 刷新某一项
        public void UpdateCell(int index)
        {
            CellInfo cellInfo = cellInfos[index - 1];
            if (cellInfo.obj != null)
            {
                float rangePos = dir == E_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                if (!IsOutRange(rangePos))
                {
                    Func(FuncCallBackFunc, cellInfo.obj);
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
            if (dir == E_Direction.Vertical)
            {
                float contentSize = (col + cellH) * Mathf.CeilToInt((float)num / lines) + paddingTop;
                contentH = contentSize;
                contentW = contentRectTrans.sizeDelta.x + paddingLeft;
                contentSize = contentSize < rectTrans.rect.height ? rectTrans.rect.height : contentSize;
                contentRectTrans.sizeDelta = new Vector2(contentW, contentSize);
                if (num != maxCount)
                {
                    contentRectTrans.anchoredPosition = new Vector2(contentRectTrans.anchoredPosition.x, 0);
                }
            }
            else
            {
                float contentSize = (row + cellW) * Mathf.CeilToInt((float)num / lines) + paddingTop;
                contentW = contentSize;
                contentH = contentRectTrans.sizeDelta.x + paddingLeft;
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
                    float rPos = dir == E_Direction.Vertical ? tempCellInfo.pos.y : tempCellInfo.pos.x;
                    if (!IsOutRange(rPos))
                    {
                        //-> 记录显示范围中的 首位index 和 末尾index
                        minIndex = minIndex == -1 ? i : minIndex; //首位index
                        maxIndex = i; // 末尾index

                        if (tempCellInfo.obj == null)
                        {
                            tempCellInfo.obj = GetPoolsObj();
                        }

                        // 记录bug，这里应该使用localPosition，避免z轴丢失导致刷新列表的时候z轴异常 为什么z轴不为0 未知
                        // tempCellInfo.obj.transform.GetComponent<RectTransform>().anchoredPosition = tempCellInfo.pos;
                        tempCellInfo.obj.transform.GetComponent<RectTransform>().localPosition = tempCellInfo.pos;
                        tempCellInfo.obj.name = i.ToString();
                        tempCellInfo.obj.SetActive(true);

                        Func(FuncCallBackFunc, tempCellInfo.obj);
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
                if (dir == E_Direction.Vertical)
                {
                    pos = cellH * Mathf.FloorToInt(i / lines) +
                          col * Mathf.FloorToInt(i / lines);
                    rowPos = cellW * (i % lines) + row * (i % lines);
                    // 为每个cell假如留白边距
                    cellInfo.pos = new Vector3(rowPos + paddingLeft, -pos - paddingTop, 0);
                }
                else
                {
                    pos = cellW * Mathf.FloorToInt(i / lines) + row * Mathf.FloorToInt(i / lines);
                    rowPos = cellH * (i % lines) + col * (i % lines);
                    cellInfo.pos = new Vector3(pos + paddingLeft, -rowPos - paddingTop, 0);
                }

                //-> 计算是否超出范围
                float cellPos = dir == E_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
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
                // 记录bug，这里应该使用localPosition，避免z轴丢失导致刷新列表的时候z轴异常 为什么z轴不为0 未知
                // cell.transform.GetComponent<RectTransform>().anchoredPosition = cellInfo.pos;
                cell.transform.GetComponent<RectTransform>().localPosition = cellInfo.pos;
                cell.gameObject.name = i.ToString();

                //-> 存数据
                cellInfo.obj = cell;
                cellInfos[i] = cellInfo;

                //-> 回调  函数
                Func(FuncCallBackFunc, cell);
            }

            maxCount = num;
            isInited = true;

            OnDragListener(Vector2.zero);
        }

        #region 扩展方法 到达指定位置

        /// 定位到第一行，也是还原到初始位置
        public void GoToOneLine()
        {
            GoToCellPos(0);
        }

        /// <summary>
        /// 通过index定位到某一单元格的坐标位置
        /// </summary>
        /// <param name="index">索引ID</param>
        public void GoToCellPos(int index)
        {
            // 如果cellInfo不存在坐标，说明没有被初始化过，当前没有数据，直接return
            if (cellInfos.Length == 0) return;
            
            // 当前索引所在行的第一个索引
            int theFirstIndex = index - index % lines;
            // 假设在第一行最大索引
            var tmpIndex = theFirstIndex + maxIndex;

            int theLastIndex = tmpIndex > maxCount - 1 ? maxCount - 1 : tmpIndex;

            // 如果最大索引就是边界的话，边界的
            if (theLastIndex == maxCount - 1)
            {
                // 余数不为0的情况下，第一个索引位置需要考虑最大数到最后显示位置的边距
                var shortOfNum = maxCount % lines == 0 ? 0 : lines - maxCount % lines;
                theFirstIndex = theLastIndex - maxIndex + shortOfNum;
            }

            Vector2 newPos = cellInfos[theFirstIndex].pos;
            if (dir == E_Direction.Vertical)
            {
                // 纵滑时定位到某一点，需要进行布局上的显示判断
                // 如果index是第0行，即index<=lines, 回到的位置应该是第一行坐标y+顶部空隙 (x,y+top)
                // index>lines,显示的index的布局应该 (x,y+col)
                var posY = index <= lines ? -newPos.y - paddingTop : -newPos.y - col;
                contentRectTrans.anchoredPosition = new Vector2(contentRectTrans.anchoredPosition.x, posY);
            }
            else
            {
                // 横向滑动时
                // 如果index是第0行，即index<=lines, 回到的位置 (x+left,y)
                // index>lines,位置应该为 (x+row,y)
                var posX = index <= lines ? -newPos.x + paddingLeft : -newPos.x + row;
                contentRectTrans.anchoredPosition = new Vector2(posX, contentRectTrans.anchoredPosition.y);
            }
        }

        #endregion


#if UNITY_EDITOR
        public void LogRecycleView()
        {
            // 拿到容器基础信息
            print("----------------------------------------------------------------------------");
            print("Direction: " + dir);
            print("Lines: " + lines);
            print(string.Format("minIndex: {0} , maxIndex: {1}", minIndex, maxIndex));
            print("Capacity: " + (maxIndex - minIndex + 1));
            print("----------------------------------------------------------------------------");
        }
#endif

        // 更新滚动区域的大小
        public void UpdateSize()
        {
            Rect rect = GetComponent<RectTransform>().rect;
            planeH = rect.height;
            planeW = rect.width;
        }

        // 滑动事件
        protected virtual void ScrollRectListener(Vector2 value)
        {
            UpdateCheck();
        }

        private void UpdateCheck()
        {
            if (cellInfos == null) return;

            // 检查超出范围
            for (int i = 0, length = cellInfos.Length; i < length; i++)
            {
                CellInfo cellInfo = cellInfos[i];
                GameObject obj = cellInfo.obj;
                Vector3 pos = cellInfo.pos;

                float rangePos = dir == E_Direction.Vertical ? pos.y : pos.x;
                // 判断是否超出显示范围
                if (IsOutRange(rangePos))
                {
                    // 把超出范围的cell 扔进 poolsObj里
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
                        // 优先从 poolsObj中 取出 （poolsObj为空则返回 实例化的cell）
                        GameObject cell = GetPoolsObj();
                        cell.transform.localPosition = pos;
                        cell.gameObject.name = i.ToString();
                        cellInfos[i].obj = cell;
                        Func(FuncCallBackFunc, cell);
                    }
                }
            }
        }

        // 判断是否超出显示范围
        protected bool IsOutRange(float pos)
        {
            Vector3 listP = contentRectTrans.anchoredPosition;
            if (dir == E_Direction.Vertical)
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


        //取出 cell
        protected virtual GameObject GetPoolsObj()
        {
            GameObject cell = null;
            if (Pool.Count > 0) cell = Pool.Pop();
            if (cell == null) cell = Instantiate(this.cell) as GameObject;

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
                Pool.Push(cell);
                SetActive(cell, false);
            }
        }

        //回调
        protected void Func(Action<GameObject, int> func, GameObject selectObject, bool isUpdate = false)
        {
            int index = int.Parse(selectObject.name);
            if (func != null)
            {
                func(selectObject, index);
            }
        }

        public void DisposeAll()
        {
            if (FuncCallBackFunc != null) FuncCallBackFunc = null;
            if (FuncOnClickCallBack != null) FuncOnClickCallBack = null;
            if (FuncOnButtonClickCallBack != null) FuncOnButtonClickCallBack = null;
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
            float normalizedPos = dir == E_Direction.Vertical
                ? scrollRect.verticalNormalizedPosition
                : scrollRect.horizontalNormalizedPosition;

            if (dir == E_Direction.Vertical)
            {
                if (contentH - rectTrans.rect.height < 10)
                {
                    SetActive(firstArrow, false);
                    SetActive(endArrow, false);
                    return;
                }
            }
            else
            {
                if (contentW - rectTrans.rect.width < 10)
                {
                    SetActive(firstArrow, false);
                    SetActive(endArrow, false);
                    return;
                }
            }

            if (normalizedPos >= 0.9)
            {
                SetActive(firstArrow, false);
                SetActive(endArrow, true);
            }
            else if (normalizedPos <= 0.1)
            {
                SetActive(firstArrow, true);
                SetActive(endArrow, false);
            }
            else
            {
                SetActive(firstArrow, true);
                SetActive(endArrow, true);
            }
        }


        public GameObject GetCellGameObject(int index)
        {
            // 为了保证拿到正确数据，根据index应该-1拿到正确数据
            return cellInfos[--index].obj;
        }

        public int GetCellIndex(GameObject obj)
        {
            // 第0号是模板，所以列表中索引应该-1
            return Convert.ToInt32(obj.name) - 1;
        }


        protected void SetActive(GameObject obj, bool isActive)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }
}
// Author: 文若
// CreateDate: 2022/10/26

using UnityEngine;
using UnityEngine.UI;

namespace WenRuo
{
    public class RecycleViewTest : MonoBehaviour
    {
        public int ListCount = 1000;
        public int gotoIndex = 0;


        public RecycleView VerticalScroll;
        public RecycleView HorizontalScroll;
        public ExpandableView ExpandScroll;
        public Button logBtn;
        public Button goToIndexBtn;

        void Start()
        {
            StartScrollView();
            logBtn.onClick.AddListener(ShowLog);
            goToIndexBtn.onClick.AddListener(GotoIndex);
        }

        public void StartScrollView()
        {
            VerticalScroll.Init(NormalCallBack);
            VerticalScroll.ShowList(ListCount);
            HorizontalScroll.Init(NormalCallBack);
            HorizontalScroll.ShowList(ListCount);
            ExpandScroll.Init(ExpandCallBack);
            ExpandScroll.ShowList("3|2|5|8");
        }

        private void NormalCallBack(GameObject cell, int index)
        {
            cell.transform.Find("Text1").GetComponent<Text>().text = index.ToString();
        }

        private void ExpandCallBack(GameObject cell, GameObject childCell, int index, int childIndex)
        {
            cell.transform.Find("Text1").GetComponent<Text>().text = "Btn : " + index.ToString();
            if (childCell != null)
            {
                childCell.transform.Find("Text1").GetComponent<Text>().text = childIndex.ToString();
            }
        }


        void ShowLog()
        {
            VerticalScroll.LogRecycleView();
        }

        void GotoIndex()
        {
            VerticalScroll.GoToCellPos(gotoIndex);
            HorizontalScroll.GoToCellPos(gotoIndex);
        }
    }
}
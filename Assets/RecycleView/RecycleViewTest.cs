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
        }

        private void NormalCallBack(GameObject cell, int index)
        {
            cell.transform.Find("Text1").GetComponent<Text>().text = index.ToString();
        }

        void ShowLog()
        {
            VerticalScroll.LogRecycleView();
        }

        void GotoIndex()
        {
            VerticalScroll.GoToCellPos(gotoIndex);
        }
    }
}
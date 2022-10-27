using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameNeon.Modules.UIModule
{
    public class RecycleViewTest : MonoBehaviour
    {
        public RecycleView VerticalScroll;
        public RecycleView HorizontalScroll_1;
        public RecycleView HorizontalScroll_2;

        public Button btn;


        void Start()
        {
            StartScrollView();
        }

        public void StartScrollView()
        {
            VerticalScroll.Init(NormalCallBack);
            VerticalScroll.ShowList(1000);


            HorizontalScroll_1.Init(NormalCallBack);
            HorizontalScroll_1.ShowList(1000);

            HorizontalScroll_2.Init(NormalCallBack);
            HorizontalScroll_2.ShowList(1000);
        }

        private void NormalCallBack(GameObject cell, int index)
        {
            cell.transform.Find("Text1").GetComponent<Text>().text = index.ToString();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private void Start()
    {
        BagMgr.GetInstance().InitData();
        UIManager.GetInstance().ShowPanel<BasePanel>("BagPanel");
    }
}
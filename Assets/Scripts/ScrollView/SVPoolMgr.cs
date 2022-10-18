// Author: 文若
// CreateDate: 2022/10/14

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class PoolData
{
    private GameObject fatherObj; // 对象挂载的父节点
    public List<GameObject> poolList; // 对象容器

    public PoolData(GameObject obj, GameObject poolObj)
    {
        // 创建一个父节点 把对象进行收纳
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.SetParent(poolObj.transform);
        poolList = new List<GameObject>();
        PushObj(obj);
    }

    public void PushObj(GameObject obj)
    {
        obj.SetActive(false);
        poolList.Add(obj);
        obj.transform.SetParent(fatherObj.transform, false);
    }

    public GameObject GetObj()
    {
        GameObject obj = null;
        // 取第一个并断开父子关系
        obj = poolList[0];
        poolList.RemoveAt(0);
        obj.SetActive(true);
        obj.transform.SetParent(null);
        return obj;
    }
}


public class SVPoolMgr
{
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    private GameObject poolObj;
    private GameObject prefab;

    public SVPoolMgr(GameObject sample)
    {
        prefab = sample;
    }

    public void GetObj(string name, UnityAction<GameObject> callBack)
    {
        // 有缓存key，并且有对象
        if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
        {
            callBack(poolDic[name].GetObj());
        }
        else
        {
            var o = GameObject.Instantiate(prefab);
            o.name = name;
            callBack(o);
            // ResMgr.GetInstance().LoadAsync<GameObject>(name, (o) =>
            // {
            //     o.name = name;
            //     callBack(o);
            // });
        }
    }

    public void PushObj(string name, GameObject obj)
    {
        if (poolObj == null)
            poolObj = new GameObject("Pool");

        if (poolDic.ContainsKey(name))
            poolDic[name].PushObj(obj);
        else
            poolDic.Add(name, new PoolData(obj, poolObj));
    }

    public void Clear()
    {
        poolDic.Clear();
        poolObj = null;
    }
}
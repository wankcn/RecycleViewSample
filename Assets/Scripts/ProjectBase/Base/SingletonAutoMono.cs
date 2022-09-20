using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//C#中 泛型知识点
//设计模式 单例模式的知识点
//继承这种自动创建的 单例模式基类 不需要我们手动去拖 或者 api去加了
//想用他 直接 GetInstance就行了
public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T GetInstance()
    {
        if( instance == null )
        {
            GameObject obj = new GameObject();
            //设置对象的名字为脚本名
            obj.name = typeof(T).ToString();
            //让这个单例模式对象 过场景 不移除
            //因为 单例模式对象 往往 是存在整个程序生命周期中的
            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<T>();
        }
        return instance;
    }

}

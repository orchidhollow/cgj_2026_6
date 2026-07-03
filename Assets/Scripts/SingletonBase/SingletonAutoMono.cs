using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动创建方式的继承Mono的单例模式基类(推荐)
/// </summary>
public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = typeof(T).ToString();
                instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手动挂载方式的继承Mono的单例模式基类(不推荐)
/// </summary>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}

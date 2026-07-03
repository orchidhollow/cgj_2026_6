using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 普通单例基类
/// </summary>
public class BaseManager<T> where T : class
{
    private static T instance;

    private static readonly object lockObj = new object();

    public static T Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    Type type = typeof(T);
                    ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                               null,
                                                               Type.EmptyTypes,
                                                               null);
                    if (info != null) instance = info.Invoke(null) as T;
                    else Debug.LogError("没有得到对应的无参构造函数");
                }
                return instance;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 包装不同委托的事件信息容器
/// </summary>
public abstract class EventInfoBase { }

/// <summary>
/// 包含泛型参数的委托容器
/// </summary>
public class EventInfo<T> : EventInfoBase
{
    public UnityAction<T> actions;
}

/// <summary>
/// 包含无参委托的委托容器
/// </summary>
public class EventInfo : EventInfoBase
{
    public UnityAction actions;
}

public class EventCenter : BaseManager<EventCenter>
{
    private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();
    private EventCenter() { }

    /// <summary>
    /// 事件触发方法 带参数
    /// </summary>
    public void EventTrigger<T>(E_EventType eventName, T obj)
    {
        if (!eventDic.ContainsKey(eventName)) return;
        (eventDic[eventName] as EventInfo<T>).actions?.Invoke(obj);
    }

    /// <summary>
    /// 事件触发方法 无参
    /// </summary>
    public void EventTrigger(E_EventType eventName)
    {
        if (!eventDic.ContainsKey(eventName)) return;
        (eventDic[eventName] as EventInfo).actions?.Invoke();
    }

    /// <summary>
    /// 事件添加监听的方法
    /// </summary>
    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic.Add(eventName, new EventInfo<T>());
        }
        (eventDic[eventName] as EventInfo<T>).actions += func;
    }

    /// <summary>
    /// 事件添加监听的方法 无参
    /// </summary>
    public void AddEventListener(E_EventType eventName, UnityAction func)
    {
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic.Add(eventName, new EventInfo());
        }
        (eventDic[eventName] as EventInfo).actions += func;
    }

    /// <summary>
    /// 事件移除监听的方法
    /// </summary>
    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions -= func;
        }
    }

    /// <summary>
    /// 事件移除监听的方法 无参
    /// </summary>
    public void RemoveEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions -= func;
        }
    }

    /// <summary>
    /// 移除所有事件的所有监听方法
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }

    /// <summary>
    /// 移除指定事件的所有监听
    /// </summary>
    public void Clear(E_EventType eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            eventDic.Remove(eventName);
        }
    }
}

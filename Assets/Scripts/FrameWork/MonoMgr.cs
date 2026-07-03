using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Mono模块管理器
/// </summary>
public class MonoMgr : SingletonAutoMono<MonoMgr>
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction lateUpdateEvent;

    /// <summary>
    /// 添加Update帧循环监听
    /// </summary>
    public void AddUpateListener(UnityAction updateFun)
    {
        updateEvent += updateFun;
    }
    /// <summary>
    /// 添加FixedUpdate帧循环监听
    /// </summary>
    public void AddFixedUpateListener(UnityAction updateFun)
    {
        fixedUpdateEvent += updateFun;
    }
    /// <summary>
    /// 添加LateUpdate帧循环监听
    /// </summary>
    public void AddLateUpateListener(UnityAction updateFun)
    {
        lateUpdateEvent += updateFun;
    }
    /// <summary>
    /// 移除Update帧循环监听
    /// </summary>
    public void RemoveUpdateListener(UnityAction updateFun)
    {
        updateEvent -= updateFun;
    }
    /// <summary>
    /// 移除FixedUpdate帧循环监听
    /// </summary>
    public void RemoveFixedUpdateListener(UnityAction updateFun)
    {
        fixedUpdateEvent -= updateFun;
    }
    /// <summary>
    /// 移除LateUpdate帧循环监听
    /// </summary>
    public void RemoveLateUpdateListener(UnityAction updateFun)
    {
        lateUpdateEvent -= updateFun;
    }
    private void Update()
    {
        updateEvent?.Invoke();
    }
    private void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }
    private void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }
}

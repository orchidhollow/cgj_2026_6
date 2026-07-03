using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// UI层级枚举
/// </summary>
public enum E_UILayer
{
    /// <summary>
    /// 底层
    /// </summary>
    Bottom,
    /// <summary>
    /// 中间层
    /// </summary>
    Middle,
    /// <summary>
    /// 高层
    /// </summary>
    top,
    /// <summary>
    /// 系统层
    /// </summary>
    system,
}

/// <summary>
/// UI管理器
/// 注意：UI预制体需要放在Resources文件夹下
/// </summary>
public class UIMgr : BaseManager<UIMgr>
{
    private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem eventSystem;

    // 层级父对象
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;

    /// <summary>
    /// 用于存储所有的面板
    /// </summary>
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    private UIMgr()
    {
        // 动态创建唯一的Canvas、EventSystem、UI摄像机

        // 同步获取摄像机 并实例化 获取摄像机预制体上的摄像机组件
        uiCamera = GameObject.Instantiate(ResourcesMgr.Instance.Load<Camera>("UI/Prefabs/UICamera")).GetComponent<Camera>();
        // UI摄像机不随场景销毁 专门用来渲染UI对象
        GameObject.DontDestroyOnLoad(uiCamera.gameObject);

        // 动态创建Canvas
        uiCanvas = GameObject.Instantiate(ResourcesMgr.Instance.Load<Canvas>("UI/Prefabs/Canvas")).GetComponent<Canvas>();
        // 使用UI摄像机来渲染
        uiCanvas.worldCamera = uiCamera;
        // 同样不随场景销毁
        GameObject.DontDestroyOnLoad(uiCanvas.gameObject);

        // 动态创建EventSystem
        eventSystem = GameObject.Instantiate(ResourcesMgr.Instance.Load<EventSystem>("UI/Prefabs/EventSystem")).GetComponent<EventSystem>();
        // 同样不随场景销毁
        GameObject.DontDestroyOnLoad(eventSystem.gameObject);

        // 找到层级父对象
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");
    }

    /// <summary>
    /// 获取层级父对象的Transform
    /// </summary>
    /// <param name="layer">层级枚举</param>
    /// <returns></returns>
    public Transform GetLayerFather(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom:
                return bottomLayer;
            case E_UILayer.Middle:
                return middleLayer;
            case E_UILayer.top:
                return topLayer;
            case E_UILayer.system:
                return systemLayer;
            default:
                return null;
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="layer">要显示层级 默认middle</param>
    /// <param name="callback">如果面板是异步加载 回调函数</param>
    /// <param name="isSyns">是否是同步加载</param>
    public void ShowPanel<T>(E_UILayer layer = E_UILayer.Middle, UnityAction<T> callback = null, bool isSyns = false) where T : BasePanel
    {
        // 获取面板名
        string panelName = typeof(T).Name;
        // 检查面板
        if (panelDic.ContainsKey(panelName))
        {
            // 执行面板显示逻辑
            panelDic[panelName].ShowMe();
            // 如果有回调函数 则回调出去
            callback?.Invoke(panelDic[panelName] as T);
            return;
        }
        // 创建面板对象
        else
        {
            // 同步加载
            if (isSyns)
            {
                GameObject res = ResourcesMgr.Instance.Load<GameObject>("UI/Prefabs/" + panelName);
                // 层级父对象
                Transform father = GetLayerFather(layer);
                // 如果没有指定层级 则默认放在Middle层
                if (father == null)
                {
                    father = middleLayer;
                }
                // 用预设创建面板到对应层级下 并保持原始位置大小
                GameObject panelObj = GameObject.Instantiate(res, father, false);
                // 获取对应UI组件返回出去
                T panel = panelObj.GetComponent<T>();
                // 显示面板时执行的默认方法
                panel.ShowMe();
                // 返回出去使用
                callback?.Invoke(panel);
                // 存到字典中
                panelDic.Add(panelName, panel);
            }
            // 异步加载
            else
            {
                ResourcesMgr.Instance.LoadAsync<GameObject>("UI/Prefabs/" + panelName, (res) =>
                {
                    // 层级父对象
                    Transform father = GetLayerFather(layer);
                    // 如果没有指定层级 则默认放在Middle层
                    if (father == null)
                    {
                        father = middleLayer;
                    }
                    // 用预设创建面板到对应层级下 并保持原始位置大小
                    GameObject panelObj = GameObject.Instantiate(res, father, false);
                    // 获取对应UI组件返回出去
                    T panel = panelObj.GetComponent<T>();
                    // 显示面板时执行的默认方法
                    panel.ShowMe();
                    // 返回出去使用
                    callback?.Invoke(panel);
                    // 存到字典中
                    panelDic.Add(panelName, panel);
                });
            }
        }
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    public void HidePanel<T>() where T : BasePanel
    {
        // 获取面板名
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            // 隐藏面板逻辑
            panelDic[panelName].HideMe();
            // 销毁面板
            GameObject.Destroy(panelDic[panelName].gameObject);
            // 从字典中移除
            panelDic.Remove(panelName);
        }
    }

    /// <summary>
    /// 获取面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    public T GetPanel<T>() where T : BasePanel
    {
        // 获取面板名
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        return null;
    }
}

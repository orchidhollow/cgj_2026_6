using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Resources资源加载管理器
/// </summary>
public class ResourcesMgr : BaseManager<ResourcesMgr>
{
    private ResourcesMgr() { }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public void LoadAsync<T>(string path, UnityAction<T> callBack) where T : Object
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path, callBack));
    }

    private IEnumerator ReallyLoadAsync<T>(string path, UnityAction<T> callBack) where T : Object
    {
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        yield return rq;
        callBack(rq.asset as T);
    }

    /// <summary>
    /// 指定卸载一个资源
    /// </summary>
    public void UnLoadAsset(Object assetToUnload)
    {
        Resources.UnloadAsset(assetToUnload);
    }

    /// <summary>
    /// 卸载没有使用的Resources资源
    /// </summary>
    public void UnloadUnusedAssets(UnityAction callback)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback)
    {
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        callback();
    }
}

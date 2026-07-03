using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ABMgr : SingletonAutoMono<ABMgr>
{
    //主包
    private AssetBundle mainAB = null;
    //主包的依赖包信息
    private AssetBundleManifest manifest = null;
    //加载过的AB包
    private Dictionary<string,AssetBundle> abDic = new Dictionary<string,AssetBundle>();
    /// <summary>
    /// AB包存放路径
    /// </summary>
    private string abPath
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    /// <summary>
    /// 主包名
    /// </summary>
    private string MainABName
    {
        get
        {   
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android"
#elif UNITY_STANDALONE_WIN
            return "PC";
#else
            return "PC";
#endif
        }
    }
    #region 同步加载

    /// <summary>
    /// 加载主包、依赖文件以及包体
    /// </summary>
    /// <param name="abName"></param>
    private void LoadAB(string abName)
    {
        //加载主包和依赖文件
        if (mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(abPath + MainABName);

        }
        if (manifest == null)
        {
            manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        string[] strs = manifest.GetAllDependencies(abName);
        for (int i = 0; i < strs.Length; i++)
        {
            if (!abDic.ContainsKey(strs[i]))
            {
                abDic.Add(strs[i], AssetBundle.LoadFromFile(abPath + strs[i]));
            }
        }
        //加载包
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(abPath + abName);
            abDic.Add(abName, ab);
        }
    }
    /// <summary>
    /// 同步加载 不指定类型
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns>Object类型</returns>
    public Object LoadRes(string abName, string resName)
    {
        LoadAB(abName);
        //加载资源
        Object obj = abDic[abName].LoadAsset(resName);
        //初步判断
        if (obj is GameObject)
        {
            return Instantiate(obj) as GameObject;
        }
        else
        {
            return obj;
        }
    }
    /// <summary>
    /// 同步加载 指定类型
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="type"></param>
    /// <returns>Object类型</returns>
    public Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadAB(abName);
        //加载资源
        Object obj = abDic[abName].LoadAsset(resName, type);
        //初步判断
        if (obj is GameObject)
        {
            return Instantiate(obj) as GameObject;
        }
        else
        {
            return obj;
        }
    }

    /// <summary>
    /// 同步加载 泛型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="type"></param>
    /// <returns>T类型</returns>
    public T LoadRes<T>(string abName, string resName, System.Type type) where T: Object
    {
        LoadAB(abName);
        //加载资源
        T obj = abDic[abName].LoadAsset<T>(resName);
        //初步判断
        if (obj is GameObject)
        {
            return Instantiate(obj);
        }
        else
        {
            return obj;
        }
    }

    #endregion

    #region 异步加载

    /// <summary>
    /// 异步加载 不指定类型
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callback"></param>
    public void LoadResAsync(string abName,string resName,UnityAction<Object> callback)
    {
        StartCoroutine(ReallyLoadResAsync(abName, resName, callback));
    }

    private IEnumerator ReallyLoadResAsync(string abName, string resName, UnityAction<Object> callback)
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName);
        yield return abr;
        if (abr.asset is GameObject)
        {
            callback(Instantiate(abr.asset) as GameObject);
        }
        else
        {
            callback(abr.asset);
        }
    }

    /// <summary>
    /// 异步加载 指定类型
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callback)
    {
        StartCoroutine(ReallyLoadResAsync(abName, resName, type, callback));
    }

    private IEnumerator ReallyLoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callback)
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName,type);
        yield return abr;
        if (abr.asset is GameObject)
        {
            callback(Instantiate(abr.asset) as GameObject);
        }
        else
        {
            callback(abr.asset);
        }
    }

    /// <summary>
    /// 异步加载 泛型
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback) where T : Object
    {   
        StartCoroutine(ReallyLoadResAsync<T>(abName, resName, callback));
    }

    private IEnumerator ReallyLoadResAsync<T>(string abName, string resName, UnityAction<T> callback) where T : Object
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abr;
        if (abr.asset is GameObject)
        {   
            callback(Instantiate(abr.asset) as T);
        }   
        else
        {
            callback(abr.asset as T);
        }
    }

    #endregion

    #region 包体卸载

    /// <summary>
    /// 卸载单个包
    /// </summary>
    /// <param name="abName"></param>
    public void UnLoadRes(string abName)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(false);
            abDic.Remove(abName);
        }
    }

    /// <summary>
    /// 卸载所有包体
    /// </summary>
    public void ClearAB()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
        mainAB = null;
        manifest = null;
    }

    #endregion
}

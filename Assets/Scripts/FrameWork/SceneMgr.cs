using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景管理器
/// </summary>
public class SceneMgr : BaseManager<SceneMgr>
{
    private SceneMgr() { }

    /// <summary>
    /// 同步切换场景
    /// </summary>
    public void LoadScene(string name, UnityAction callback = null)
    {
        SceneManager.LoadScene(name);
        callback?.Invoke();
    }

    /// <summary>
    /// 异步切换场景
    /// </summary>
    public void LoadSceneAsyn(string name, UnityAction callback = null)
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadSceneAsyn(name, callback));
    }

    private IEnumerator ReallyLoadSceneAsyn(string name, UnityAction callback = null)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        while (!ao.isDone)
        {
            EventCenter.Instance.EventTrigger<float>(E_EventType.E_SceneLoadChange, ao.progress);
            yield return 0;
        }
        EventCenter.Instance.EventTrigger<float>(E_EventType.E_SceneLoadChange, 1);
        callback?.Invoke();
    }
}

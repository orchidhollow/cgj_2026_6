using UnityEngine;

/// <summary>
/// 关卡管理器：管理摄像机切换
/// 根据当前星球切换角色摄像机或广角摄像机
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Cameras")]
    /// <summary>角色跟随摄像机（正常视角）</summary>
    public Camera playerCamera;
    /// <summary>广角摄像机（小行星等小星球使用）</summary>
    public Camera wideCamera;

    /// <summary>小行星名称（匹配时切换广角摄像机）</summary>
    private const string SmallPlanetName = "小行星";

    /// <summary>单例</summary>
    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        // 默认使用角色摄像机
        SwitchToPlayerCamera();
    }

    /// <summary>
    /// 根据星球名称切换摄像机
    /// 由 Planet.OnTriggerEnter2D 调用
    /// </summary>
    /// <param name="planetName">星球名称</param>
    public void SwitchCameraByPlanet(string planetName)
    {
        if (planetName == SmallPlanetName)
            SwitchToWideCamera();
        else
            SwitchToPlayerCamera();
    }

    /// <summary>切换到角色跟随摄像机</summary>
    public void SwitchToPlayerCamera()
    {
        if (playerCamera != null) playerCamera.enabled = true;
        if (wideCamera != null) wideCamera.enabled = false;
    }

    /// <summary>切换到广角摄像机</summary>
    public void SwitchToWideCamera()
    {
        if (playerCamera != null) playerCamera.enabled = false;
        if (wideCamera != null)
        {
            wideCamera.enabled = true;
        }
        else
        {
            Debug.LogWarning("[LevelManager] wideCamera 未赋值！请在 Inspector 中拖入广角摄像机");
        }
    }
}

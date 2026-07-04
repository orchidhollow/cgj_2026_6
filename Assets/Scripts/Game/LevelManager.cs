using UnityEngine;

/// <summary>
/// 关卡管理器：管理摄像机切换、起点/终点、关卡流程
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Cameras")]
    /// <summary>角色跟随摄像机（正常视角）</summary>
    public Camera playerCamera;
    /// <summary>广角摄像机（小行星等小星球使用）</summary>
    public Camera wideCamera;

    [Header("Level Points")]
    /// <summary>关卡起点（Player 初始化位置）</summary>
    public Transform startPoint;
    /// <summary>关卡终点（Player 触碰后触发过关）</summary>
    public Transform endPoint;

    [Header("References")]
    /// <summary>Player 引用</summary>
    public Player player;

    /// <summary>小行星名称（匹配时切换广角摄像机）</summary>
    private const string SmallPlanetName = "小行星";

    /// <summary>单例</summary>
    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 默认使用角色摄像机
        SwitchToPlayerCamera();
        // 将 Player 初始化到起点
        InitPlayerToStart();
    }

    /// <summary>
    /// 将 Player 传送到起点位置
    /// </summary>
    void InitPlayerToStart()
    {
        if (player != null && startPoint != null)
        {
            player.transform.position = startPoint.position;
        }
    }

    /// <summary>
    /// 终点触发：Player 碰到终点时调用
    /// 由终点物体的 Trigger 调用
    /// </summary>
    public void OnReachEndPoint()
    {
        Debug.Log("[LevelManager] 到达终点！关卡完成");
        // TODO: 过关逻辑（加载下一关、播放动画等）
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

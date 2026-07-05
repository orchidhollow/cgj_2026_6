using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
/// <summary>
/// 关卡管理器：管理摄像机切换、起点/终点、关卡流程
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Cameras")]
    /// <summary>角色跟随摄像机（正常视角）</summary>
    /// <summary>广角摄像机（小行星等小星球使用）</summary>
    public Transform playerTransform;
    public Dictionary<string,Transform> planetTransforms = new Dictionary<string, Transform>();
    public CinemachineVirtualCamera PlayerCamera;
    public CinemachineVirtualCamera planetCamera;
    public float virtualCameraSize = 10f;
    public float targetSize = 15f;
    public float CameraSmoothSpeed = 30f;
    public float CameraRotateSpeed = 5f;
    public float RotateAngle = 0f;
    private bool PlayerCameraSizeOut = false;
    private bool PlayerCameraSizeIn = false;
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
        playerTransform = player.transform;
        planetTransforms.Add("小行星",GameObject.Find("小行星").transform);
    }

    void Start()
    {
        // 默认使用角色摄像机
        SwitchToPlayerCamera();
        // 将 Player 初始化到起点
        InitPlayerToStart();
        // 播放游戏配乐和环境音
        FMODAudioMgr.Instance?.PlayBGMGame();
        FMODAudioMgr.Instance?.PlayAmbience();
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
   
    void LateUpdate()
    {
        //RotateAngle = player.transform.eulerAngles.z;
        //float currentSmoothAngle = Mathf.LerpAngle(PlayerCamera.m_Lens.Dutch,RotateAngle,Time.deltaTime * CameraRotateSpeed);
        //PlayerCamera.m_Lens.Dutch = Mathf.DeltaAngle(RotateAngle, currentSmoothAngle);
        
        if(PlayerCameraSizeOut)
        {
            if(Mathf.Abs(PlayerCamera.m_Lens.OrthographicSize - targetSize) > 0.01f)
                PlayerCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                PlayerCamera.m_Lens.OrthographicSize,
                targetSize,
                Time.deltaTime * CameraSmoothSpeed); 
            else
                {
                    PlayerCamera.m_Lens.OrthographicSize = targetSize;
                    PlayerCameraSizeOut = false;
                }
        }
        else if(PlayerCameraSizeIn)
        {
            if(Mathf.Abs(PlayerCamera.m_Lens.OrthographicSize - virtualCameraSize) > 0.01f)
                PlayerCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                    PlayerCamera.m_Lens.OrthographicSize,
                     virtualCameraSize,
                      Time.deltaTime * CameraSmoothSpeed);
            else
            {
                PlayerCamera.m_Lens.OrthographicSize = virtualCameraSize;
                PlayerCameraSizeIn = false;
            }
        }
    }
    /// <summary>
    /// 终点触发：Player 碰到终点时调用
    /// 由终点物体的 Trigger 调用
    /// </summary>
    public void OnReachEndPoint()
    {
        Debug.Log("[LevelManager] 到达终点！关卡完成");
        GameManager.Instance.OnGameOver();
        // TODO: 过关逻辑（加载下一关、播放动画等）
    }

    /// <summary>
    /// 根据星球名称切换摄像机
    /// 由 Planet.OnTriggerEnter2D 调用
    /// </summary>
    /// <param name="planetName">星球名称</param>
   

    /// <summary>切换到角色跟随摄像机</summary>
    public void SwitchToPlayerCamera()
    {
        if(PlayerCamera != null && playerTransform != null)
        {
            PlayerCameraSizeIn = true;
            //PlayerCamera.Follow = playerTransform;
            planetCamera.Priority = 0;
        }
    }

    /// <summary>切换到广角摄像机</summary>
    public void SwitchToWideCamera(string planetName = SmallPlanetName)
    {
        if(PlayerCamera != null )
        {
           
            if(planetTransforms.TryGetValue(planetName, out Transform wideCameraTransform))
            {
                 var body = PlayerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if(body!= null)
                {
                     body.m_XDamping = 4.0f;
                     body.m_YDamping = 4.0f;
                     body.m_ScreenX = 0.5f;
                 }
                PlayerCameraSizeOut = true;
                //PlayerCamera.Follow = wideCameraTransform;
                planetCamera.Priority = 11;
            }
            else
            {
                Debug.Log("[LevelManager] 未找到小行星的 Transform，请确保在 Awake 中正确添加到 planetTransforms 字典中");
            }
        }
        else
        {
            Debug.LogWarning("[LevelManager] wideCamera 未赋值！请在 Inspector 中拖入广角摄像机");
        }
    }
}

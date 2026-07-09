using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Cinemachine;
/// <summary>
/// 关卡管理器：管理摄像机切换、起点/终点、关卡流程
/// </summary>
public class LevelManager : MonoBehaviour
{
    public GameObject chengong;
    
    public GameObject shibai;
    
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
    /// <summary>关卡终点（需要挂 Collider2D，Is Trigger ✓）</summary>
    public Collider2D endPoint;

    [Header("References")]
    /// <summary>Player 引用</summary>
    public Player player;

    [Header("开场视频")]
    /// <summary>视频渲染面板</summary>
    public RawImage videoRawImage;
    /// <summary>视频播放器</summary>
    public VideoPlayer videoPlayer;

    /// <summary>小行星名称（匹配时切换广角摄像机）</summary>
    private const string SmallPlanetName = "小行星";

    /// <summary>单例</summary>
    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        if (player == null)
            playerTransform = player.transform;

        var asteroid = GameObject.Find("小行星");
        if (asteroid != null)
            planetTransforms.Add("小行星", asteroid.transform);

        // 注册终点碰撞检测
        if (endPoint != null)
        {
            var trigger = endPoint.GetComponent<EndPoint>();
            if (trigger == null)
                trigger = endPoint.gameObject.AddComponent<EndPoint>();
        }
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
        // 播放开场视频
        PlayIntroVideo();
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
    /// 播放开场视频：显示 RawImage，播放 VideoPlayer，播放结束后关闭
    /// </summary>
    void PlayIntroVideo()
    {
        if (videoRawImage == null || videoPlayer == null) return;

        // 显示视频面板
        videoRawImage.gameObject.SetActive(true);

        // 注册播放结束回调
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        // 开始播放
        videoPlayer.Play();
        isVideoPlaying = true;
        // 播放 CG 音频
        FMODAudioMgr.Instance?.PlayCG();
    }

    /// <summary>
    /// 视频播放结束：关闭 RawImage
    /// </summary>
    void OnVideoEnd(VideoPlayer vp)
    {
        isVideoPlaying = false;
        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);
        // 视频结束，播放游戏配乐
        FMODAudioMgr.Instance?.PlayBGMGame();
    }
   
    /// <summary>视频是否正在播放</summary>
    private bool isVideoPlaying = false;

    void Update()
    {
        // 按返回键跳过视频
        if (isVideoPlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipVideo();
        }
    }

    /// <summary>跳过视频</summary>
    void SkipVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
        OnVideoEnd(videoPlayer);
    }

    void LateUpdate()
    {
        //RotateAngle = player.transform.eulerAngles.z;
        //float currentSmoothAngle = Mathf.LerpAngle(PlayerCamera.m_Lens.Dutch,RotateAngle,Time.deltaTime * CameraRotateSpeed);
        //PlayerCamera.m_Lens.Dutch = Mathf.DeltaAngle(RotateAngle, currentSmoothAngle);
        
        /*if(PlayerCameraSizeOut)
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
                    PlayerCameraSizeIn = true;
                }
        }
        if(PlayerCameraSizeIn)
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
                PlayerCameraSizeOut = true;
            }
        }
        */
        
    }
    /// <summary>
    /// 终点触发：Player 碰到终点时调用
    /// 由终点物体的 Trigger 调用
    /// </summary>
    public void OnReachEndPoint()
    {
        Debug.Log("[LevelManager] 到达终点！关卡完成");
        chengong.SetActive(true);
        //GameManager.Instance.OnGameOver();
    }
    
   
    /// <summary>
    /// 玩家死亡：将 Player 重置回起点位置
    /// 由 Player.OnPlayerDie 调用
    /// </summary>
    public void HandlePlayerDeath(Player player)
    {
        if (player == null || startPoint == null) return;

        // 显示失败 UI，3 秒后隐藏
        StartCoroutine(ShowShibaiDelayed());

        // 重置玩家位置到起点
        player.transform.position = startPoint.position;
        player.transform.rotation = startPoint.rotation;
    }

    IEnumerator ShowShibaiDelayed()
    {
        shibai.SetActive(true);
        yield return new WaitForSeconds(3f);
        shibai.SetActive(false);
    }

    /// <summary>
    /// 根据星球名称切换摄像机
    /// 由 Planet.OnTriggerEnter2D 调用
    /// </summary>
    /// <param name="planetName">星球名称</param>
   

    /// <summary>切换到角色跟随摄像机</summary>
    public void SwitchToPlayerCamera()
    {
        if (PlayerCamera != null && playerTransform != null)
        {
            PlayerCameraSizeIn = true;
            PlayerCamera.Follow = playerTransform;
            if (planetCamera != null)
                planetCamera.Priority = 0;
        }
    }

    /// <summary>切换到广角摄像机</summary>
    public void SwitchToWideCamera(string planetName = SmallPlanetName)
    {
        if (PlayerCamera != null)
        {
            if (planetTransforms.TryGetValue(planetName, out Transform wideCameraTransform) || planetName == "小行星")
            {
                var body = PlayerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (body != null)
                {
                    body.m_XDamping = 4.0f;
                    body.m_YDamping = 4.0f;
                    body.m_ScreenX = 0.5f;
                }
                PlayerCameraSizeOut = true;
                if (planetCamera != null)
                    planetCamera.Priority = 11;
            }
            else
            {
                if (planetCamera != null)
                    planetCamera.Priority = 0;
                Debug.Log("[LevelManager] 未找到小行星的 Transform，请确保在 Awake 中正确添加到 planetTransforms 字典中");
            }
        }
        else
        {
            Debug.LogWarning("[LevelManager] wideCamera 未赋值！请在 Inspector 中拖入广角摄像机");
        }
    }
}

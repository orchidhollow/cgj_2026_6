/// <summary>
/// 游戏流程管理器：控制游戏开始、结束等核心流程
/// 支持两种模式：
/// 1. BaseManager 纯代码单例模式（有 BaseManager 时）
/// 2. MonoBehaviour 模式（无 BaseManager 时，自动切换）
/// 不依赖 EventCenter，直接处理场景重开
/// 保留 EventCenter 事件监听作为备选
/// </summary>
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// 检查是否存在 BaseManager，如果不存在则使用 MonoBehaviour 模式
#if BASE_MANAGER_EXISTS
public class GameManager : BaseManager<GameManager>
#else
public class GameManager : MonoBehaviour
#endif
{
#if !BASE_MANAGER_EXISTS
    /// <summary>单例实例（MonoBehaviour 模式）</summary>
    public static GameManager Instance { get; private set; }
#endif

    /// <summary>死亡后重开延迟时间（秒）</summary>
    public float restartDelay = 2f;

    /// <summary>是否正在处理死亡重开，防止重复调用</summary>
    private bool isHandlingDeath = false;

#if !BASE_MANAGER_EXISTS
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
#else
    private GameManager()
    {
        // 构造时注册事件监听（BaseManager 模式）
        EventCenter.Instance?.AddEventListener(E_EventType.E_GameStart, OnGameStart);
        EventCenter.Instance?.AddEventListener(E_EventType.E_GameOver, OnGameOverEvent);
    }
#endif

    // ===== 游戏开始 =====

    /// <summary>
    /// 游戏开始：加载指定关卡场景
    /// 由 StartButton 调用，用于从主菜单进入游戏
    /// 兼容 EventCenter 事件触发
    /// </summary>
    /// <param name="gameSceneName">游戏场景名称</param>
    public void OnGameStart(string gameSceneName)
    {
        Debug.Log("GameManager: 游戏开始 - 加载场景 " + gameSceneName);

        // 确保时间缩放正常
        Time.timeScale = 1f;

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 加载游戏场景
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// 游戏开始（无参数版本，重开当前场景）
    /// 由 StartButton 调用，用于重新开始当前关卡
    /// 兼容 EventCenter 事件触发
    /// </summary>
    public void OnGameStart()
    {
        Debug.Log("GameManager: 游戏开始 - 重开当前场景");

        // 确保时间缩放正常
        Time.timeScale = 1f;

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 获取当前场景名称并重新加载
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// 游戏开始（EventCenter 事件版本）
    /// 由 EventCenter 事件触发，用于 BaseManager 模式
    /// 注意：方法名与上面不同，避免重载冲突
    /// </summary>
    public void OnGameStartEvent(object data = null)
    {
        Debug.Log("GameManager: Game Start (Event)");

        // 确保时间缩放正常
        Time.timeScale = 1f;

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 使用 SceneMgr 加载（如果可用），否则使用 SceneManager
        if (SceneMgr.Instance != null)
        {
            SceneMgr.Instance.LoadSceneAsyn("Game", () =>
            {
                LevelManager.Instance?.SwitchToPlayerCamera();
            });
        }
        else
        {
            SceneManager.LoadScene("Game");
        }
    }

    // ===== 游戏结束 =====

    /// <summary>
    /// 游戏结束（无参数版本）
    /// 由 LevelManager 调用，默认返回主菜单
    /// </summary>
    public void OnGameOver()
    {
        Debug.Log("GameManager: 游戏结束 - 返回主菜单");

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 返回主菜单（默认场景名）
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// 游戏结束：处理关卡失败或玩家主动放弃
    /// 由 LevelManager 调用，用于返回主菜单或显示结束界面
    /// </summary>
    /// <param name="returnToMainMenu">是否直接返回主菜单</param>
    public void OnGameOver(bool returnToMainMenu)
    {
        Debug.Log("GameManager: 游戏结束");

        // 重置死亡处理标记
        isHandlingDeath = false;

        if (returnToMainMenu)
        {
            // 恢复时间并返回主菜单
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // 不返回主菜单，暂停游戏等待UI处理
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// 游戏结束（带场景名称版本）
    /// 由 LevelManager 调用，指定返回的场景名称
    /// </summary>
    /// <param name="targetSceneName">目标场景名称</param>
    public void OnGameOver(string targetSceneName)
    {
        Debug.Log("GameManager: 游戏结束 - 返回场景 " + targetSceneName);

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 加载指定场景
        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// 游戏结束（EventCenter 事件版本）
    /// 由 EventCenter 事件触发，用于 BaseManager 模式
    /// 注意：方法名与上面不同，避免重载冲突
    /// </summary>
    public void OnGameOverEvent(object data = null)
    {
        Debug.Log("GameManager: Game Over (Event)");

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 使用 SceneMgr 加载（如果可用），否则使用 SceneManager
        if (SceneMgr.Instance != null)
        {
            SceneMgr.Instance.LoadSceneAsyn("GameOver");
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    // ===== 死亡处理 =====

    /// <summary>
    /// 处理玩家死亡：延迟后重开当前场景
    /// 由 Player.OnPlayerDie 调用
    /// </summary>
    /// <param name="player">死亡的玩家</param>
    public void HandlePlayerDeath(Player player)
    {
        // 防止重复处理死亡
        if (isHandlingDeath) return;
        isHandlingDeath = true;

        Debug.Log("GameManager: Player Death - 准备重开");

        // 停止游戏时间，冻结所有物理和动画更新
        Time.timeScale = 0f;

        // 启动延迟重开协程
        StartCoroutine(RestartAfterDelay());
    }

    /// <summary>
    /// 延迟重开协程：等待指定时间后重开当前场景
    /// </summary>
    IEnumerator RestartAfterDelay()
    {
        // 使用真实时间等待，不受 Time.timeScale 影响
        yield return new WaitForSecondsRealtime(restartDelay);

        Debug.Log("GameManager: 重开场景");

        // 重置死亡处理标记
        isHandlingDeath = false;

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 重开当前场景
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu(string mainMenuSceneName)
    {
        // 重置死亡处理标记
        isHandlingDeath = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

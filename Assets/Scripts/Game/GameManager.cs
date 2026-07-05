using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏管理器：统一管理开始界面、游戏流程、死亡重开
///
/// 使用方式：
/// 1. GameStart 场景放空物体挂 GameManager
/// 2. Inspector 中拖入 startButton 和 quitButton
/// 3. 自动绑定点击事件，不需要额外脚本
/// 4. DontDestroyOnLoad 跨场景保留
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>单例</summary>
    public static GameManager Instance { get; private set; }

    [Header("开始界面按钮")]
    /// <summary>开始游戏按钮</summary>
    public Button startButton;
    /// <summary>退出游戏按钮</summary>
    public Button quitButton;

    [Header("场景名称")]
    /// <summary>游戏场景名称</summary>
    public string gameSceneName = "Game";
    /// <summary>开始界面场景名称</summary>
    public string menuSceneName = "GameStart";

    public string overSceneName = "GameOver";

    void Awake()
    {
        // 单例 + 跨场景保留
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 绑定按钮事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            startButton.onClick.AddListener(() => FMODAudioMgr.Instance?.PlayUIConfirm());
            AddHoverSound(startButton);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            quitButton.onClick.AddListener(() => FMODAudioMgr.Instance?.PlayUIConfirm());
            AddHoverSound(quitButton);
        }

        // 播放开始界面音乐
        FMODAudioMgr.Instance?.PlayBGMMain();
    }

    /// <summary>给按钮添加悬浮音效</summary>
    void AddHoverSound(Button button)
    {
        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => FMODAudioMgr.Instance?.PlayUISelect());
        trigger.triggers.Add(entry);
    }
    
    // ===== 开始界面 =====

    /// <summary>开始游戏（停掉开始界面音乐）</summary>
    public void StartGame()
    {
        Time.timeScale = 1f;
        FMODAudioMgr.Instance?.StopMusic();
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>退出游戏</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ===== 游戏流程 =====

    /// <summary>游戏结束：返回开始界面</summary>
    public void OnGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(overSceneName);
    }
}

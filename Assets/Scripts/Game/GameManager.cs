/// <summary>
/// 游戏流程管理器：控制游戏开始、结束等核心流程
/// 纯代码单例，不依赖 MonoBehaviour，通过 BaseManager 自动创建
/// </summary>
using UnityEngine;
public class GameManager : BaseManager<GameManager>
{
    private GameManager()
    {
        // 构造时注册事件监听
        EventCenter.Instance.AddEventListener(E_EventType.E_GameStart, OnGameStart);
        EventCenter.Instance.AddEventListener(E_EventType.E_GameOver, OnGameOver);
    }

    public void OnGameStart()
    {
        Debug.Log("GameManager: Game Start");
        SceneMgr.Instance.LoadSceneAsyn("Game", () =>
        {
            LevelManager.Instance.SwitchToPlayerCamera();
        });
    }

    public void OnGameOver()
    {
        Debug.Log("GameManager: Game Over");
        SceneMgr.Instance.LoadSceneAsyn("GameOver");
    }
}

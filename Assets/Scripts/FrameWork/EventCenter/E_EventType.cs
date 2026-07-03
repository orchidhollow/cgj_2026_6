/// <summary>
/// 事件类型枚举
/// 定义游戏中所有可监听的事件类型
/// </summary>
public enum E_EventType
{
    /// <summary>场景切换时进度获取</summary>
    E_SceneLoadChange,

    /// <summary>展示下一张卡（参数: RoundCard）</summary>
    E_ShowNextCard,

    /// <summary>回合结束</summary>
    E_RoundFinished,
}

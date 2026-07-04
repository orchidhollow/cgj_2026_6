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

    /// <summary>Anchor 发射（参数: Anchor）</summary>
    E_AnchorFired,
    /// <summary>Anchor 命中物体（参数: Anchor）</summary>
    E_AnchorHit,
    /// <summary>Anchor 到达回收（参数: Anchor）</summary>
    E_AnchorArrived,

    /// <summary>玩家状态变化（参数: Player）</summary>
    E_PlayerStateChanged,
    /// <summary>玩家受击（参数: Player）</summary>
    E_PlayerHit,
    /// <summary>玩家死亡（参数: Player）</summary>
    E_PlayerDeath,
    E_GameStart,
    E_GameOver,
}

using UnityEngine;

/// <summary>
/// 控制配置：集中管理 Player 和 Anchor 的数值参数
/// 右键 Create > Config > Control Config 生成资产
/// 运行时修改 SO 资产上的数值即可实时生效
/// </summary>
[CreateAssetMenu(fileName = "ControlConfig", menuName = "Config/Control Config")]
public class ControlConfig : ScriptableObject
{
    [Header("Player Movement")]
    [Tooltip("角色在星球表面的移动速度")]
    public float moveSpeed = 5f;
    [Tooltip("跳跃时沿背离星球方向的弹射力度")]
    public float jumpForce = 8f;

    [Header("Player Anchor")]
    [Tooltip("发射船锚后锁定玩家输入的时长（秒），超时后恢复正常控制")]
    public float anchorFireDuration = 0.3f;

    [Header("Player Hit")]
    [Tooltip("受击后的无敌时长（秒），期间不会再次触发受击")]
    public float hitDuration = 0.5f;

    [Header("Anchor")]
    [Tooltip("船锚的飞行速度（单位/秒）")]
    public float anchorSpeed = 20f;
    [Tooltip("船锚命中后拉动角色的速度（单位/秒）")]
    public float anchorPullSpeed = 15f;
    [Tooltip("角色到达锚点的判定距离，小于此值视为到达")]
    public float anchorArriveDistance = 0.5f;
    [Tooltip("船锚最大飞行距离，超过则原路返回")]
    public float anchorMaxDistance = 20f;
    [Tooltip("可钩取物体所在的 Layer 名称，需要在 Tag & Layer 中配置")]
    public string hitchLayerName = "Hitch";
}

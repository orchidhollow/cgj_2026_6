# Anchor 抓钩位移功能设计（v2）

## 功能概述

鼠标点击方向 → 角色投掷 Anchor → Anchor 碰到物体后勾住 → 把角色拉过去 → 到达后 Anchor 回收

---

## 可复用的框架组件

| 框架组件 | 用途 |
|----------|------|
| `PoolMgr` | Anchor 对象池，`GetObj` 获取 / `PushObj` 回收，避免 GC |
| `EventCenter` | 发射/命中/到达事件广播，后续接音效、UI |
| `E_EventType` | 新增 Anchor 相关事件枚举 |
| `MusicMgr` | 播放投掷/命中音效（可选） |

---

## 改动文件清单

### 1. `Assets/Scripts/FrameWork/EventCenter/E_EventType.cs`（修改）

新增 3 个事件类型：

```csharp
public enum E_EventType
{
    E_SceneLoadChange,
    E_ShowNextCard,
    E_RoundFinished,

    // ===== 新增 =====
    /// <summary>Anchor 发射（参数: Anchor）</summary>
    E_AnchorFired,
    /// <summary>Anchor 命中物体（参数: Anchor）</summary>
    E_AnchorHit,
    /// <summary>Anchor 到达回收（参数: Anchor）</summary>
    E_AnchorArrived,
}
```

---

### 2. `Assets/Scripts/Game/Anchor.cs`（重写）

当前状态：空脚本

改动后：使用 PoolMgr 回收，命中后广播事件

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Anchor : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float pullForce = 30f;
    public float arriveDistance = 0.5f;
    public float maxLifetime = 3f;

    private Rigidbody2D rb;
    private Player owner;
    private bool hasHit;
    private float timer;

    public void Init(Player player, Vector2 direction)
    {
        owner = player;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = direction.normalized * speed;
        GetComponent<CircleCollider2D>().isTrigger = true;
        hasHit = false;
        timer = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.GetComponent<Player>() != null) return;

        hasHit = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        // 广播命中事件
        EventCenter.Instance.EventTrigger(E_EventType.E_AnchorHit, this);
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer > maxLifetime)
        {
            Recycle();
            return;
        }

        if (!hasHit || owner == null) return;

        // 拉动角色
        Vector2 dir = ((Vector2)transform.position - (Vector2)owner.transform.position).normalized;
        owner.GetComponent<Rigidbody2D>().AddForce(dir * pullForce, ForceMode2D.Force);

        // 到达判定
        float dist = Vector2.Distance(transform.position, owner.transform.position);
        if (dist < arriveDistance)
        {
            // 广播到达事件
            EventCenter.Instance.EventTrigger(E_EventType.E_AnchorArrived, this);
            Recycle();
        }
    }

    void Recycle()
    {
        hasHit = false;
        rb.isKinematic = false;
        rb.linearVelocity = Vector2.zero;
        PoolMgr.Instance.PushObj(gameObject);
    }
}
```

---

### 3. `Assets/Scripts/Game/Player.cs`（新增投掷逻辑）

当前状态：52 行，无 Anchor 相关代码

**新增字段：**

```csharp
[Header("Anchor")]
public string anchorPrefabPath = "Prefabs/Anchor";  // Resources 路径
```

**在 Update 中新增：**

```csharp
// 投掷 Anchor（新增）
if (Input.GetMouseButtonDown(0))
{
    FireAnchor();
}
```

**新增方法：**

```csharp
void FireAnchor()
{
    // 从对象池获取
    GameObject go = PoolMgr.Instance.GetObj(anchorPrefabPath);
    go.transform.position = transform.position;
    Anchor anchor = go.GetComponent<Anchor>();

    // 鼠标世界坐标
    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mouseWorld.z = 0;
    Vector2 dir = (mouseWorld - transform.position).normalized;

    anchor.Init(this, dir);

    // 广播发射事件
    EventCenter.Instance.EventTrigger(E_EventType.E_AnchorFired, anchor);
}
```

---

### 4. Anchor 预制体配置

```
Anchor (GameObject)
├── Rigidbody2D (gravityScale=0, isKinematic=false)
├── CircleCollider2D (isTrigger=true, radius=0.2)
├── SpriteRenderer (临时图标)
├── Anchor.cs (脚本)
└── PoolObjMaxNum.cs (maxNum=10)  ← 对象池上限
```

预制体放入 `Resources/Prefabs/` 目录，PoolMgr 通过路径加载。

---

## 流程图

```
鼠标左键
    │
    ▼
PoolMgr.GetObj("Prefabs/Anchor")
    │
    ▼
anchor.Init(player, direction)
EventCenter 触发 E_AnchorFired
    │
    ▼
Anchor 飞行中（不受重力）
    │
    ├── 碰到物体 → 停下
    │   EventCenter 触发 E_AnchorHit
    │       │
    │       ▼
    │   AddForce 拉动角色
    │       │
    │       ▼
    │   距离 < 0.5
    │   EventCenter 触发 E_AnchorArrived
    │       │
    │       ▼
    │   PoolMgr.PushObj 回收
    │
    └── 超时(3s) → PoolMgr.PushObj 回收
```

---

## 注意事项

1. **拉动与常规移动冲突**：Player.FixedUpdate 中切线 velocity 会和拉力对冲，后续可加 `isBeingPulled` 状态跳过常规移动
2. **星球重力干扰**：拉动期间重力仍在，可临时降低重力或加大 pullForce
3. **穿透问题**：Anchor 速度过快可开 Rigidbody2D CCD（Continuous Collision Detection）
4. **对象池路径**：Anchor 预制体必须放在 `Resources/Prefabs/` 下，名字和 `anchorPrefabPath` 一致

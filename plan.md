# FMOD 音频单例方案

## 一个单例够吗？

**够用。** 所有音效通过事件路径（字符串）调用，不需要在 Inspector 中配置任何引用。

```
FMODAudioMgr.Instance.PlayJump();          // 一行调用
FMODAudioMgr.Instance.PlayAnchorHit(true); // 带参数
```

## 需要在 Inspector 中配置吗？

**不需要。** 所有事件路径硬编码在脚本中，和 FMOD Studio 的事件路径一一对应。

唯一要求：FMOD Studio 中的事件路径必须和脚本中的字符串一致。

## 但是有一个问题

`SingletonAutoMono<T>` 会自动创建 GameObject。如果场景中没有挂 `FMODAudioMgr` 的物体，第一次调用时会自动创建一个。

**推荐做法：** 在场景中手动放一个空物体挂上 `FMODAudioMgr`，这样可以控制初始化时机。

## 需要做的

1. 新建 `Assets/Scripts/FrameWork/FMODAudioMgr.cs`
2. 场景中放空物体挂上该脚本（可选，不挂也能自动创建）
3. FMOD Studio 中按路径创建对应事件

## 文件清单

| 操作 | 文件 |
|------|------|
| 新建 | `Assets/Scripts/FrameWork/FMODAudioMgr.cs` |

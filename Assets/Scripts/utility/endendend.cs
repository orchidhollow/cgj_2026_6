using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // 仅编辑器环境生效的命名空间
#endif

public class endendend : MonoBehaviour
{
    // 按钮点击时触发的方法（需绑定到Button的OnClick事件）
    public void OnQuitButtonClick()
    {
        Debug.Log("点击了退出游戏按钮，执行关闭游戏逻辑");

        // 区分编辑器/打包环境的退出逻辑
#if UNITY_EDITOR
        // Unity编辑器中：停止播放模式（模拟退出）
        EditorApplication.isPlaying = false;
#else
        // 打包后（PC/移动端）：真正退出游戏
        Application.Quit();
#endif
    }
}
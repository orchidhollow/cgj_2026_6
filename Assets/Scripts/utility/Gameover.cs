using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro命名空间

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUIPrefab; // 关联GameOverUI预制体
    private GameObject currentGameOverUI; // 当前实例化的UI对象

    void Start()
    {
        // 确保初始时UI不显示
        if (currentGameOverUI != null)
            Destroy(currentGameOverUI);
    }

    // 游戏结束触发函数（例如玩家死亡、关卡失败时调用）
    public void TriggerGameOver()
    {
        // 实例化Game Over UI
        currentGameOverUI = Instantiate(gameOverUIPrefab, transform.parent); // 挂载到场景根节点或Canvas下

        // 自动居中（关键！）
        RectTransform rt = currentGameOverUI.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); // 锚点设为画布中心
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); // 对齐点为中心
        rt.sizeDelta = Vector2.zero; // 重置尺寸偏移

        // 暂停游戏时间（可选）
        Time.timeScale = 0f;
    }

    // 重新开始游戏
    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢复时间流逝
        Destroy(currentGameOverUI); // 销毁UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 重新加载当前场景
    }

    // 返回主菜单（需提前在Build Settings中配置主菜单场景索引）
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        Destroy(currentGameOverUI);
        SceneManager.LoadScene(0); // 假设主菜单是第0个场景
    }
}
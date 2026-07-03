using UnityEngine;

public class EnergyFrameEvents : MonoBehaviour
{
    // 由动画事件调用
    public void EnergyDepleted()
    {
        // 1. 扣血
        HeartHUD heartHUD = FindObjectOfType<HeartHUD>();
        heartHUD?.ChangeHP(-1);

        // 2. 如果血空了，这里也可以直接 GameOver
        // if (heartHUD.CurrentHP <= 0) SceneManager.LoadScene("GameOver");
    }
}

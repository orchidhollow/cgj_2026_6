using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;   // 如果用 UI 路线

public class CgFade : MonoBehaviour
{
    [Header("CG 节点")]
    [SerializeField] private CanvasGroup canvasGroup; // UI 路线
    
    

    [Header("淡入/淡出时间")]
    [SerializeField] private float fadeIn = 1f;
    [SerializeField] private float fadeOut = 1f;

    /* 外部唯一调用：渐现 → 停留 → 渐隐 */
    public void Play(System.Action onComplete = null)
    {
        // 保证从 0 开始
        canvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0.5f, fadeIn))        // 淡入
           .AppendInterval(1.5f)                            // CG 停留 2 秒（可改）
           .Append(canvasGroup.DOFade(0.5f, fadeOut))       // 淡出
           .OnComplete(() => onComplete?.Invoke());       // 回调
    }
}

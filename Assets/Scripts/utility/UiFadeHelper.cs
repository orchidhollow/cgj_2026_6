using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UiFadeHelper : MonoBehaviour
{
    [Header("把面板根节点拖进来")]
    public CanvasGroup canvasGroup;

    [Header("淡入淡出时间")]
    public float fadeTime = 0.35f;

    public void FadeIn()          // 渐入
    {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, fadeTime).SetEase(Ease.OutQuad);
    }

    public void FadeOut()         // 渐出
    {
        canvasGroup.DOFade(0, fadeTime).SetEase(Ease.InQuad);
    }
}

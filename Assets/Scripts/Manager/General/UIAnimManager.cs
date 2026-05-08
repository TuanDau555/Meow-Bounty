using DG.Tweening;
using UnityEngine;

public class UIAnimManager : MonoBehaviour 
{
    #region Parameters

    [Header("General")]
    [SerializeField] private float _fadeTime = 1f;

    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    
    #endregion

    #region Anim

    public void PaneFadeIn()
    {
        canvasGroup.alpha = 0f;
        rectTransform.transform.localPosition = new Vector3(0f, -1000f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, 0f), _fadeTime, false).SetEase(Ease.OutBounce);
        canvasGroup.DOFade(1, _fadeTime);
    }

    public void PanelFadeOut()
    {
        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, -1000f), _fadeTime, false).SetEase(Ease.InOutQuint);
        canvasGroup.DOFade(0, _fadeTime);    
    }
    
    #endregion
}
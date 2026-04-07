using UnityEngine;
using DG.Tweening;

public class UIFader : MonoBehaviour
{
    #region Parameters

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [SerializeField] private float fadeInDelay = 0f;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;
    [SerializeField] private Ease fadeOutEase = Ease.InQuad;

    [Header("Bounce Settings (Fade In)")]
    [SerializeField] private bool useBounce = true;
    [SerializeField] private float bounceScale = 1.1f;
    [SerializeField] private float bounceDuration = 0.25f;
    [SerializeField] private Ease bounceEase = Ease.OutBack;

    [Header("Slide Settings (Fade Out)")]
    [SerializeField] private SlideDirection slideDirection = SlideDirection.Down;
    [SerializeField] private float slideOffset = 80f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Vector3 _originalPos;

    #endregion

    #region Execute

    private void Awake()
    {
        _canvasGroup = GetComponentInParent<CanvasGroup>();
        if (!_canvasGroup)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _rectTransform = GetComponent<RectTransform>();
        _originalPos = _rectTransform.anchoredPosition;
    }
    
    #endregion
    #region Fade In/Out
        
    /// <summary>
    /// Fade in with optional bounce.
    /// </summary>
    public void FadeIn()
    {
        // gameObject.SetActive(true);

        _canvasGroup.alpha = 0f;
        _rectTransform.localScale = useBounce ? Vector3.one * 0.7f : Vector3.one;

        Sequence seq = DOTween.Sequence();

        // Bounce
        if (useBounce)
        {
            seq.Append(_rectTransform.DOScale(bounceScale, bounceDuration).SetEase(bounceEase));
            seq.Join(_canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase).SetDelay(fadeInDelay));
            seq.Append(_rectTransform.DOScale(1f, bounceDuration * 0.5f).SetEase(Ease.OutQuad));
        }
        else
        {
            seq.Append(_canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase).SetDelay(fadeInDelay));
        }
    }

    /// <summary>
    /// Fade out with slide.
    /// </summary>
    public void FadeOut(bool deactivateAfter = true)
    {
        _canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = _originalPos;

        Vector2 slideTarget = _originalPos;

        switch (slideDirection)
        {
            case SlideDirection.Left:
                slideTarget += Vector2.left * slideOffset;
                break;
            case SlideDirection.Right:
                slideTarget += Vector2.right * slideOffset;
                break;
            case SlideDirection.Up:
                slideTarget += Vector2.up * slideOffset;
                break;
            case SlideDirection.Down:
                slideTarget += Vector2.down * slideOffset;
                break;
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(_canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
        seq.Join(_rectTransform.DOAnchorPos(slideTarget, fadeOutDuration).SetEase(fadeOutEase));

        if (deactivateAfter)
        {
            seq.OnComplete(() =>
            {
                // gameObject.SetActive(false);
                _rectTransform.anchoredPosition = _originalPos;
            });
        }
    }
    
    #endregion
}

public enum SlideDirection
{
    None,
    Left,
    Right,
    Up,
    Down
}
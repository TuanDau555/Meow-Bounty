using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTweener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Parameters

    [Header("General")]
    [SerializeField] private TweenType tweenType = TweenType.ScaleBounce;
    [SerializeField] private float _duration = 0.15f;

    [Header("Scale Bounce")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    [SerializeField] private Vector3 clickScale = new Vector3(0.9f, 0.9f, 1f);
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Fade")]
    [SerializeField] private float hoverAlpha = 0.8f;
    [SerializeField] private float clickAlpha = 0.6f;

    [Header("Move")]
    [SerializeField] private Vector3 hoverOffset = new Vector3(0, 10, 0);
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    [Header("Rotate")]
    [SerializeField] private float hoverRotation = 15f;
    [SerializeField] private float clickRotation = 360f;
    [SerializeField] private Ease rotateEase = Ease.OutBack;
    
    private Vector3 _defaultScale;
    private Vector3 _defaultPos;
    private Vector3 _defaultRotation;
    private CanvasGroup _canvasGroup;
    private Image _image;

    #endregion

    #region Execute

    private void Awake()
    {
        _defaultScale = transform.localScale;
        _defaultPos = transform.localPosition;
        _defaultRotation = transform.localEulerAngles;
    }

    #endregion

    #region Interact Interfaces

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (tweenType)
        {
            case TweenType.ScaleBounce:
                transform.DOScale(clickScale, _duration * 0.5f).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        transform
                            .DOScale(_defaultScale, _duration)
                            .SetEase(scaleEase);
                    });
                break;

            case TweenType.Fade:
                _canvasGroup
                    .DOFade(clickAlpha, _duration * 0.5f)
                    .OnComplete(() => _canvasGroup.DOFade(1f, _duration));
                break;

            case TweenType.Move:
                transform
                    .DOLocalMove(_defaultPos + hoverOffset * 0.5f, _duration * 0.5f)
                    .OnComplete(() => transform.DOLocalMove(_defaultPos, _duration));
                break;

            case TweenType.Rotate:
            transform
                .DOLocalRotate(
                    _defaultRotation + new Vector3(0, 0, clickRotation),
                    _duration * 2f,
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    transform.localEulerAngles = _defaultRotation;
                });
            break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (tweenType)
        {
            case TweenType.ScaleBounce:
                transform.DOScale(hoverScale, _duration).SetEase(scaleEase);
                break;

            case TweenType.Fade:
                _canvasGroup.DOFade(hoverAlpha, _duration);
                break;

            case TweenType.Move:
                transform
                .DOLocalMove(
                    _defaultPos + hoverOffset,
                    _duration
                )
                .SetEase(moveEase);
                break;

            case TweenType.Rotate:
            transform
                .DOLocalRotate(
                    _defaultRotation + new Vector3(0, 0, clickRotation),
                    _duration * 2f,
                    RotateMode.FastBeyond360
                )
                .SetEase(rotateEase);
            break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
         switch (tweenType)
        {
            case TweenType.ScaleBounce:
                transform.DOScale(_defaultScale, _duration).SetEase(scaleEase);
                break;

            case TweenType.Fade:
                _canvasGroup.DOFade(1f, _duration);
                break;

            case TweenType.Move:
                transform.DOLocalMove(_defaultPos, _duration).SetEase(moveEase);
                break;

            case TweenType.Rotate:
            transform
                .DOLocalRotate(_defaultRotation, _duration)
                .SetEase(rotateEase);
            break;
        }
    }
    
    #endregion
}

public enum TweenType
{
    None,
    ScaleBounce,
    Fade,
    Move,
    Rotate,
    Color
}
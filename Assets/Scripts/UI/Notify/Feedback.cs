using TMPro;
using UnityEngine;

public class Feedback : Singleton<Feedback>
{
    #region Parameters

    [Header("Reference")]
    [SerializeField] private FeedbackTextSO feedbackTextSO;

    [Header("UI")]
    [SerializeField] private CanvasGroup feedbackCanvas;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Config")]
    [SerializeField] private float autoHideTime;

    private float _hideTimer;
    private bool _isShowingUI;

    #endregion

    #region Execute

    private void Start()
    {
        HideFeedback();
    }

    private void Update()
    {
        if(_isShowingUI)
        {
            _hideTimer -= Time.deltaTime;
            if(_hideTimer <= 0f)
            {
                HideFeedback();
            }
        }
    }

    #endregion

    #region Feedback

    /// <summary>
    /// Show lobby State or time count down
    /// </summary>
    /// <param name="state">State in lobby or time</param>
    public void ShowLobbyState(string state)
    {
        if(feedbackText == null || feedbackCanvas == null) return;

        feedbackText.text = feedbackTextSO.lobbyStateText + state;

        feedbackCanvas.alpha = 1;

        StartCountDown();
    }

    public void MicError()
    {
        if(feedbackText == null || feedbackCanvas == null) return;

        feedbackText.text = feedbackTextSO.micErrorText;

        feedbackCanvas.alpha = 1;

        StartCountDown();
    }

    #endregion

    #region Show/Hide

    private void HideFeedback()
    {
        if(feedbackCanvas != null)
        {
            feedbackCanvas.alpha = 0;
        }

        _isShowingUI = false;
    }

    private void StartCountDown()
    {
        _hideTimer = autoHideTime;
        _isShowingUI = true;
    }
    
    #endregion
}
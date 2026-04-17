using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour 
{
    #region Parameters

    [SerializeField] private CanvasGroup chatCanvas;
    [SerializeField] private CanvasGroup inputCanvas;
    [SerializeField] private TMP_InputField chatInput;

    private bool _isInputOpen = false;

    #endregion

    #region Execute

    private void Start()
    {
        chatCanvas.alpha = 1;
        chatCanvas.interactable = false;
        chatCanvas.blocksRaycasts = false;
        
        HideInput();
    }

    private void Update()
    {
        if (InputManager.Instance.IsChatPressed() && !_isInputOpen)
        {
            ShowInput();
        }

        if(InputManager.Instance.IsEnterPressed() && _isInputOpen)
        {
            TrySendMessage();
        }

        if(InputManager.Instance.IsClosedChat() && _isInputOpen)
        {
            HideInput();
        }
    }

    #endregion

    #region Input

    private void ShowInput()
    {
        _isInputOpen = true;
        
        inputCanvas.alpha = 1;
        inputCanvas.interactable = true;
        inputCanvas.blocksRaycasts = true;

        chatInput.text = string.Empty;
        chatInput.ActivateInputField(); // Focus on input field

        ChatManager.Instance.KeepVisible();
        
        InputManager.Instance.DisableGameInput();
    }

    private void HideInput()
    {
        _isInputOpen = false;
        
        inputCanvas.alpha = 0;
        inputCanvas.interactable = false;
        inputCanvas.blocksRaycasts = false;

        chatInput.DeactivateInputField();

        ChatManager.Instance.StartFadeTimer();

        InputManager.Instance.EnableGameInput();
    
    }

    private void TrySendMessage()
    {
        string message = chatInput.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            string playerName = ServiceLocator.ProfileService?.DisplayName + ">" ?? "Player>";
            ChatManager.Instance.SendChatMessage(message, playerName);
        }

        HideInput();
        
    }
    
    #endregion
}
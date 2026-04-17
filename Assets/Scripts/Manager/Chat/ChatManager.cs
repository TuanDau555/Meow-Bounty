using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class ChatManager : SingletonNetwork<ChatManager> 
{
    #region Parameters

    [Header("Ref")]
    [SerializeField] private ChatMessage chatMessagePrefab;
    [SerializeField] private Transform chatContent;
    [SerializeField] private CanvasGroup chatCanvasGroup;
    [SerializeField] private ScrollRect scrollRect; 

    [Header("Settings")]
    [SerializeField] private int maxMessage = 30;
    [SerializeField] private float messageVisibleDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;

    private int _messageCount = 0;
    private CountdownTimer _visibleTimer;
    private CountdownTimer _fadeTimer;
    private bool _isFading = false;
    private float _lastSendTime;
    private float _coolDown = 0.5f;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        _visibleTimer = new CountdownTimer(messageVisibleDuration);
        _fadeTimer = new CountdownTimer(fadeDuration);

        _visibleTimer.OnTimerStop += HandleVisibleTimerStop;

        _fadeTimer.OnTimerStop += HandleFadeTimerStop;
    }

    private void Update()
    {
        _visibleTimer.Tick(Time.deltaTime);
        _fadeTimer.Tick(Time.deltaTime);

        if (_isFading)
        {
            float ratio = _fadeTimer.Progress;

            chatCanvasGroup.alpha = ratio;
        }
        AutoScroll();
    }

    #endregion

    #region Events

    private void HandleFadeTimerStop()
    {
        chatCanvasGroup.alpha = 0;
        _isFading = false;
    }

    private void HandleVisibleTimerStop()
    {
        _isFading = true;
        _fadeTimer.Start();
    }

    #endregion

    #region Add Message

    private void AddMessage(string name, string message, ulong senderClientId)
    {
        if(_messageCount >= maxMessage)
        {
            Destroy(chatContent.GetChild(0).gameObject);
            _messageCount--;
        }
        
        ChatMessage chatMessage = Instantiate(chatMessagePrefab, chatContent);

        bool isMine = senderClientId == NetworkManager.Singleton.LocalClientId;
        
        chatMessage.SetName(name, isMine);
        chatMessage.SetText(message, isMine);
        _messageCount++;
        
        SetChatVisible(true, true);
        RestartVisibleTimer();
    }

    public void SendChatMessage(string message, string from = null)
    {
        if(Time.time - _lastSendTime < _coolDown) return;

        _lastSendTime = Time.time;
        
        if(string.IsNullOrWhiteSpace(message)) return;

        if(message.Length > 100)
        {
            message = message.Substring(0, 100);
        }
        
        SendChatMessageServerRpc(from, message);
    }
    
    #endregion

    #region Visible

    public void KeepVisible()
    {
        _visibleTimer.Stop();
        _fadeTimer.Stop();
        _isFading = false;
        SetChatVisible(true, true);
    }

    public void StartFadeTimer() => RestartVisibleTimer();

    private void RestartVisibleTimer()
    {
        _fadeTimer.Stop();
        _isFading = false;

        chatCanvasGroup.alpha = 1;
        chatCanvasGroup.interactable = true;
        chatCanvasGroup.blocksRaycasts = true;

        _visibleTimer.Reset();
        _visibleTimer.Start();
    }
    
    private void SetChatVisible(bool visible, bool instant)
    {
        if(instant)
        {
            chatCanvasGroup.alpha = visible ? 1f : 0f;
            return;
        }
        if (!visible)
        {
            RestartVisibleTimer();
        }
    }

    private void AutoScroll()
    {
        if(scrollRect.verticalNormalizedPosition <= 0.05f)
        {
            StartCoroutine(ScrollNextFrame());
        }
    }
    
    private IEnumerator ScrollNextFrame()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    #endregion
    
    #region RPC

    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string name, string message, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        ReceiveChatMessageClientRpc(name, message, senderClientId);
    }
    
    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string name, string message, ulong senderClientId)
    {
        AddMessage(name, message, senderClientId);
    }
    
    #endregion
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionUI : MonoBehaviour 
{
    #region Parameters

    [Header("UI")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI mapNameText;

    #endregion

    #region Execute

    private void Start()
    {
        nextBtn.onClick.AddListener(MapSelectionManager.Instance.Next);
        previousBtn.onClick.AddListener(MapSelectionManager.Instance.Previous);
        MapSelectionManager.Instance.OnMapChanged += RefreshUI;

        RefreshUI(MapSelectionManager.Instance.SelectedMap);

    }

    private void OnEnable()
    {
        if(ServiceLocator.GameLobbyService != null)
        {
            ServiceLocator.GameLobbyService.OnLocalLobbyUpdated += HandleLobbyUpdated;
            RefreshHostUI();
        }
    }
    
    private void OnDisable()
    {
        nextBtn.onClick.RemoveListener(MapSelectionManager.Instance.Next);
        previousBtn.onClick.RemoveListener(MapSelectionManager.Instance.Previous);
        MapSelectionManager.Instance.OnMapChanged -= RefreshUI;
        
        if(ServiceLocator.GameLobbyService != null)
        {
            ServiceLocator.GameLobbyService.OnLocalLobbyUpdated -= HandleLobbyUpdated;
        }
    }

    #endregion

    #region Event

    private void HandleLobbyUpdated(object sender, LobbyData e)
    {
        RefreshHostUI();
    }
    
    #endregion
    
    #region UI

    private void RefreshUI(MapInfo mapInfo)
    {
        thumbnail.sprite = mapInfo.mapThumbnail;
        mapNameText.text = mapInfo.mapName;
    }

    private void RefreshHostUI()
    {
        var lobby = ServiceLocator.GameLobbyService;

        if(lobby == null)
        {
            nextBtn.gameObject.SetActive(false);
            previousBtn.gameObject.SetActive(false);
            return;
        }

        bool isHost = ServiceLocator.GameLobbyService.GetHostAuthority.IsHost;
        nextBtn.gameObject.SetActive(isHost);
        previousBtn.gameObject.SetActive(isHost);
 
    }
    
    #endregion
}
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

        bool isHost = ServiceLocator.GameLobbyService.HostAuthority.IsHost;
        nextBtn.gameObject.SetActive(isHost);
        previousBtn.gameObject.SetActive(isHost);

        RefreshUI(MapSelectionManager.Instance.SelectedMap);
    }

    private void OnDisable()
    {
        nextBtn.onClick.RemoveListener(MapSelectionManager.Instance.Next);
        previousBtn.onClick.RemoveListener(MapSelectionManager.Instance.Previous);
        MapSelectionManager.Instance.OnMapChanged -= RefreshUI;
        
    }

    #endregion

    #region UI

    private void RefreshUI(MapInfo mapInfo)
    {
        thumbnail.sprite = mapInfo.mapThumbnail;
        mapNameText.text = mapInfo.mapName;
    }
    
    #endregion
}
using System;
using UnityEngine;

public class MapSelectionManager : Singleton<MapSelectionManager>
{
    #region Parameters

    [SerializeField] private MapSelectionSO mapSelectionSO;

    private int _currentIndex = 0;

    public MapInfo SelectedMap => mapSelectionSO.maps[_currentIndex];

    public event Action<MapInfo> OnMapChanged;
    
    #endregion

    #region Choosing func
    
    public void Next()
    {
        if(!ServiceLocator.GameLobbyService.HostAuthority.IsHost) return;
        
        _currentIndex = (_currentIndex + 1) % mapSelectionSO.maps.Count;
        OnMapChanged?.Invoke(SelectedMap);

        // sync to lobby
        _ = ServiceLocator.GameLobbyService.UpdateSelectedMapAsync(SelectedMap.sceneName);
    }

    public void Previous()
    {
        if(!ServiceLocator.GameLobbyService.HostAuthority.IsHost) return;
        
        _currentIndex--;
        if(_currentIndex < 0)
        {
            _currentIndex = mapSelectionSO.maps.Count - 1;
        }
        OnMapChanged?.Invoke(SelectedMap);

        _ = ServiceLocator.GameLobbyService.UpdateSelectedMapAsync(SelectedMap.sceneName);
    }

    #endregion

    #region SET

    public void SetMapBySceneName(string sceneName)
    {
        int index = mapSelectionSO.maps.FindIndex(m => m.sceneName == sceneName);
        if(index < 0) return;

        _currentIndex = index;
        OnMapChanged?.Invoke(SelectedMap);
    }
    
    #endregion
}
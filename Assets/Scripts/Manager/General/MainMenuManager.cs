using System;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    #region Parameter
    [Header("Ref")]
    [SerializeField] private CharacterSelection characterSelection;

    private bool _initialized = false;

    #endregion
    
    #region Execute

    private void Start()
    {
        var profile = ServiceLocator.ProfileService;

        profile.OnProfileReady += OnProfileReady;

        if(profile.PlayerData != null)
        {
            OnProfileReady(null, EventArgs.Empty);
        }
    }

    #endregion

    #region Profile Ready
    private void OnProfileReady(object sender, EventArgs e)
    {
        // Only init one time
        if(_initialized) return;
        _initialized = true;

        var profile = ServiceLocator.ProfileService;
        profile.OnProfileReady -= OnProfileReady;

        characterSelection.Init(profile);
        Debug.Log("Character selection initialized");
    }

    private void OnDestroy()
    {
        var profile = ServiceLocator.ProfileService;
        if(profile != null)
        {
            profile.OnProfileReady -= OnProfileReady;
        }
    }

    #endregion

    
}
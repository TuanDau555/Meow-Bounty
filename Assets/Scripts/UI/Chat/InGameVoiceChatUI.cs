using Unity.Services.Vivox;
using UnityEngine;

public class InGameVoiceChatUI : MonoBehaviour 
{
    #region Parameters
     
    [SerializeField] private GameObject micOnIcon;
    [SerializeField] private GameObject micOffIcon;

    #endregion

    #region Execute

    private void Update()
    {
        if (InputManager.Instance.IsUseVoiceChat())
        {
            ToggleMute();
        }
    }

    private void OnEnable()
    {   
        RefreshUI();
    }

    #endregion
    
    #region Toggle

    private void ToggleMute()
    {
        VivoxManager.Instance.ToggleMute();
        RefreshUI();
    }

    private void RefreshUI()
    {
        bool isMuted = VivoxManager.Instance.IsMuted;

        micOnIcon.SetActive(!isMuted);
        micOffIcon.SetActive(isMuted);
    }
    
    #endregion
}
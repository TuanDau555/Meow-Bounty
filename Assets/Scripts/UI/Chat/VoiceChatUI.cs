using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class VoiceChatUI : MonoBehaviour
{
    #region Parameters

    [SerializeField] private Button muteBtn;
    [SerializeField] private Button onBtn;
    [SerializeField] private GameObject micOnIcon;
    [SerializeField] private GameObject micOffIcon;
    [SerializeField] private Transform participantContainer;
    [SerializeField] private VoiceParticipantUI participantPrefab;

    #endregion

    #region Execute

    private void OnEnable()
    {
        muteBtn.onClick.AddListener(ToggleMute);
        onBtn.onClick.AddListener(ToggleMute);
        
        VivoxManager.Instance.OnParticipantAdded += HandleParticipantAdded;
        VivoxManager.Instance.OnParticipantRemoved += HandleParticipantRemoved;

        RefreshUI();
    }

    private void OnDestroy()
    {
        muteBtn.onClick.RemoveListener(ToggleMute);
        onBtn.onClick.RemoveListener(ToggleMute);

        VivoxManager.Instance.OnParticipantAdded -= HandleParticipantAdded;
        VivoxManager.Instance.OnParticipantRemoved -= HandleParticipantRemoved;
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

    #region Events

    private void HandleParticipantAdded(VivoxParticipant participant)
    {
        var item = Instantiate(participantPrefab, participantContainer);
        item.SetUp(participant);
    }

    private void HandleParticipantRemoved(VivoxParticipant participant)
    {
        foreach(Transform child in participantContainer)
        {
            var ui = child.GetComponent<VoiceParticipantUI>();
            if(ui != null && ui.Participant == participant)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }
    
    #endregion
}
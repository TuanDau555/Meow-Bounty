using TMPro;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceParticipantUI : MonoBehaviour
{
    #region Parameters

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject speakingIndicator;

    public VivoxParticipant Participant { get; private set; }
        
    #endregion

    #region Execute
    
    #endregion

    public void SetUp(VivoxParticipant participant)
    {
        Participant= participant;
        nameText.text = participant.DisplayName;

        participant.ParticipantSpeechDetected += RefreshSpeakingUI;
        participant.ParticipantMuteStateChanged += RefreshSpeakingUI;

        RefreshSpeakingUI();
    }

    private void RefreshSpeakingUI()
    {
        speakingIndicator.SetActive(
            Participant.SpeechDetected && !Participant.IsMuted
        );
    }
}
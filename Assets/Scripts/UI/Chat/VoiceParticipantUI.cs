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
        Debug.Log($"Speech: {Participant.SpeechDetected}");

        participant.ParticipantSpeechDetected += () =>
        {
            Debug.Log($"{participant.DisplayName} is speaking");
        };
        
        RefreshSpeakingUI();
    }

    private void OnDestroy()
    {
        if (Participant == null) return;

        Participant.ParticipantSpeechDetected -= RefreshSpeakingUI;
        Participant.ParticipantMuteStateChanged -= RefreshSpeakingUI;
    }

    private void RefreshSpeakingUI()
    {
        speakingIndicator.SetActive(
            Participant.SpeechDetected && !Participant.IsMuted
        );
    }
}
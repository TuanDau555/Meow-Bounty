using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxManager : SingletonPersistent<VivoxManager>
{
    #region Parameters

    public bool IsLoggedIn { get; private set; }
    public bool IsMuted { get; private set; }

    // Events
    public event Action OnLoggedIn;
    public event Action OnLoggedOut;
    public event Action <VivoxParticipant> OnParticipantAdded;
    public event Action <VivoxParticipant> OnParticipantRemoved;

    private string _currentChannelName;
    private bool _isInitialized = false;
    
    #endregion

    #region Init

    public async Task InitialLizeAysnc()
    {
        if(_isInitialized) return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        _isInitialized = true;
        
        Debug.Log("UGS + Vivox Init");
    }
    
    #endregion

    #region Login

    public async Task LoginAsync(string displayName)
    {
        try
        {
            if(!_isInitialized)
            {
                await InitialLizeAysnc();
            }

            var options = new LoginOptions
            {
                DisplayName = displayName,
                EnableTTS = false // we don't need this
            };

            await VivoxService.Instance.LoginAsync(options);

            IsLoggedIn = true;

            VivoxService.Instance.UnmuteInputDevice();
            IsMuted = false;
            
            OnLoggedIn?.Invoke();

            Debug.Log($"Vivox logged in: {displayName}");
            
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox login failed: {e.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        if(!IsLoggedIn) return;

        try
        {
            await LeaveChannelAsync();
            await VivoxService.Instance.LogoutAsync();

            IsLoggedIn = false;
            OnLoggedOut?.Invoke();

            Debug.Log("Vivox logged out");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox logout failed: {e.Message}");
        }
    }
    
    #endregion

    #region Channel

    public async Task JoinGroupChannelAsync(string channelName)
    {
        if(!IsLoggedIn) return;

        if(_currentChannelName == channelName) return;

        try
        {
            await LeaveChannelAsync(); // dounble check ?

            _currentChannelName = channelName;

            VivoxService.Instance.ParticipantAddedToChannel += HandleParticipanntAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel += HandleParticipanntRemoved;

            await VivoxService.Instance.JoinGroupChannelAsync(
                channelName,
                ChatCapability.AudioOnly
            );

            VivoxService.Instance.UnmuteInputDevice();

            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.All);

            IsMuted = false;
            
            Debug.Log($"[Vivox] Setup complete");
            Debug.Log($"[Vivox] Active Channels: {VivoxService.Instance.ActiveChannels.Count}");
            Debug.Log($"[Vivox] Is Input Muted: {VivoxService.Instance.IsInputDeviceMuted}");
            Debug.Log($"[Vivox] Active Input Device: {VivoxService.Instance.ActiveInputDevice?.DeviceName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Join Channel Failed: {e.Message}");
        }
    }

    public async Task LeaveChannelAsync()
    {
        if(string.IsNullOrEmpty(_currentChannelName)) return;

        try
        {

            await VivoxService.Instance.LeaveAllChannelsAsync();

            VivoxService.Instance.ParticipantAddedToChannel -= HandleParticipanntAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel -= HandleParticipanntRemoved;

            _currentChannelName = null;

            Debug.Log("Left Vivox channel");
        }
        catch (Exception e)
        {
            Debug.LogError($"Leave Channel failed: {e.Message}");
        }
    }
    
    #endregion

    #region Mic

    public void SetMute(bool mute)
    {
        IsMuted = mute;

        if (mute)
        {
            VivoxService.Instance.MuteInputDevice();
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
        }

        Debug.Log($"Mic Muted: {mute}");
    }

    public void ToggleMute()
    {
        SetMute(!IsMuted);
    }
    
    #endregion

    #region Events

    private void HandleParticipanntAdded(VivoxParticipant participant)
    {
        Debug.Log($"Participant joined: {participant.DisplayName}");
        OnParticipantAdded?.Invoke(participant);
    }

    private void HandleParticipanntRemoved(VivoxParticipant participant)
    {
        Debug.Log($"Participant left: {participant.DisplayName}");
        OnParticipantRemoved?.Invoke(participant);
    }
    
    #endregion
    
}
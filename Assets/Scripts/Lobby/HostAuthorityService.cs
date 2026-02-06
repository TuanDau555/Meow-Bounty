using System;
using Unity.Services.Authentication;

public class HostAuthorityService : IHostAuthority
{
    #region 
    public bool IsHost { get; private set; }

    public string HostId { get; private set; }

    public event EventHandler OnHostGained;
    public event EventHandler OnHostLost;

    private readonly IAuthenticationService authService;
    #endregion

    #region Constructor
    public HostAuthorityService(IAuthenticationService authService)
    {
        this.authService = authService;
    }
    #endregion

    #region Host Permission
    public void UpdateHost(string newHostId)
    {
        bool wasHost = IsHost;

        HostId = newHostId;
        IsHost = authService.PlayerId == newHostId;

        if(!wasHost && IsHost) 
            OnHostGained?.Invoke(this, EventArgs.Empty);

        if(wasHost && !IsHost)
            OnHostLost?.Invoke(this, EventArgs.Empty);
    }

    public bool CanKick(string targetPlayerId)
    {
        if(!IsHost) return false;

        // Host is not able to kick themself 
        return authService.PlayerId != targetPlayerId; 
    }
    #endregion
}

using System;

public interface IHostAuthority
{
    bool IsHost { get; }
    string HostId { get; }

    event EventHandler OnHostGained;
    event EventHandler OnHostLost;

    void UpdateHost(string newHostId);

    bool CanKick(string targetPlayerId);
}

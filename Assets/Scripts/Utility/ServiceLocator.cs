public static class ServiceLocator
{
    public static PlayerProfileService ProfileService { get; private set; }
    public static IGameLobbyService GameLobbyService { get; private set; }

    public static bool HasProfile => ProfileService != null;
    public static bool HasLobbyService => GameLobbyService != null;

    /// <summary>
    /// Player Data Ready to use
    /// </summary>
    public static void InitProFile()
    {
        if(ProfileService != null) return;

        ProfileService = new PlayerProfileService();
    }

    /// <summary>
    /// PlayFab ready
    /// </summary>
    public static void InitLobby(IHostAuthority hostAuthority)
    {
        if (GameLobbyService != null) return;

        GameLobbyService = new GameLobbyService(ProfileService, hostAuthority);
    }
}
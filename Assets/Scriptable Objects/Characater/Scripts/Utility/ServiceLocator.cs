public static class ServiceLocator
{
    public static IGameLobbyService GameLobbyService { get; private set; }

    public static bool HasLobbyService => GameLobbyService != null;

    public static void InitLobby(PlayerProfileService profile, IHostAuthority hostAuthority)
    {
        if (GameLobbyService != null) return;

        GameLobbyService = new GameLobbyService(profile, hostAuthority);
    }
}
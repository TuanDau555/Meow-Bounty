using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class show kick Btn to Host
/// </summary>
public class PlayerInfoUI : MonoBehaviour
{
    #region Parameter
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI characterText;
    [SerializeField] private GameObject readyState;
    [SerializeField] private Button kickBtn;

    private LobbyPlayerData playerData;
    private IGameLobbyService gameLobbyService;
    private IHostAuthority hostAuthority;
    #endregion

    #region Asign Data
    public void SetUp(LobbyPlayerData data, IGameLobbyService gameLobbyService, IHostAuthority hostAuthority)
    {
        this.playerData = data;
        this.gameLobbyService = gameLobbyService;
        this.hostAuthority = hostAuthority;

        playerNameText.text = data.displayName;
        characterText.text = data.characterId;

        if (!data.isReady)
        {
            readyState.GetComponent<Image>().color = Color.red;
        }

        RenderKickBtn();
    }

    private void RenderKickBtn()
    {
        bool canKick = hostAuthority.CanKick(playerData.playerId);

        kickBtn.gameObject.SetActive(canKick);

        if(!canKick) return;

        kickBtn.onClick.RemoveAllListeners();
        kickBtn.onClick.AddListener(OnKickClicked);
    }

    private void OnKickClicked()
    {
        gameLobbyService.KickPlayerAsync(playerData.playerId);
    }
    #endregion

    #region GET

    public GameObject GetReadyState()
    {
        return readyState;
    }
    
    #endregion
}

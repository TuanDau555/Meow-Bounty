using UnityEngine;
using Unity.Netcode;

public class GamePauseManager : NetworkBehaviour 
{
    #region Execute

    private void Update()
    {
        if (InputManager.Instance.IsPausedGame())
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        PauseGameUI.Instance.Show();
    }

    #endregion
}
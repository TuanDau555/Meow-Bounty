using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
     [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;
    public GameObject settingPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    void HideAll()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    // ===== MAIN MENU =====
    public void ShowMainMenu()
    {
        HideAll();
        mainMenuPanel.SetActive(true);
    }

    public void OpenLobby()
    {
        HideAll();
        lobbyPanel.SetActive(true);
    }

    public void OpenSetting()
    {
        HideAll();
        settingPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ===== LOBBY =====
    public void CreateRoom()
    {
        Debug.Log("Create Room");
        // Dùng gắn network
    }

    public void FindRoom()
    {
        Debug.Log("Find Room");
        // Dùng gắn network
    }
}

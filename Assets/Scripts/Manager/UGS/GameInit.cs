using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameInit : MonoBehaviour
{
    private void Start()
    {
        AuthManager.Instance.OnAuthReady += HandleAuthReady;
    }
    private void OnDisable()
    {
        AuthManager.Instance.OnAuthReady -= HandleAuthReady;
    }

    private void HandleAuthReady(object sender, EventArgs e)
    {
        SceneManager.LoadSceneAsync("Main Menu");
    }
}
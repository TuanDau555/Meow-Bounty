using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject nameInputUI;

    private void Start()
    {
        OnProfileReady();
    }

    private void OnProfileReady()
    {
        PlayerProfileService profileService = new PlayerProfileService();
        if (!profileService.HasDisplayName())
        {
            nameInputUI.SetActive(true);
        }
        else
        {
            nameInputUI.SetActive(false); 
        }
    }
}
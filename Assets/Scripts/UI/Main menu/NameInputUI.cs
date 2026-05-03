using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button confirmBtn;

    public event Action OnDisplayNameUpdated;

    private void Awake()
    {
        confirmBtn.onClick.AddListener(OnConfirmClicked);
    }

    private void OnConfirmClicked()
    {
        string name = nameInput.text.Trim();
        var profile = ServiceLocator.ProfileService.PlayerData;
        if (!IsValidName(name))
        {
            Debug.Log("Name must be 3–16 characters");
            return;
        } 

        confirmBtn.interactable = false;

        PlayFabManager.SetDisplayName(
            name,
            OnSuccess,
            OnError
        );
        profile.name = name;
        PlayFabManager.SaveProfile(profile);
    }

    private void OnSuccess()
    {
        Debug.Log("Display name set");
        OnDisplayNameUpdated?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnError(string error)
    {
        confirmBtn.interactable = true;
        Debug.LogError("Invalid name, please try differecne name");
    }

    private bool IsValidName(string name)
    {
        return name.Length >= 3 && name.Length <= 16;
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button confirmBtn;

    private void Awake()
    {
        confirmBtn.onClick.AddListener(OnConfirmClicked);
    }

    private void OnConfirmClicked()
    {
        string name = nameInput.text.Trim();

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
    }

    private void OnSuccess()
    {
        Debug.Log("Display name set");
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
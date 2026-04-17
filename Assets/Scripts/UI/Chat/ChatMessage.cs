using TMPro;
using UnityEngine;

/// <summary>
/// This script is need to be attach to chat messsage prefab
/// </summary>
public class ChatMessage : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Color ownerMessageColor = Color.green;
    [SerializeField] private Color otherMessageColor = Color.black;

    public void SetText(string text, bool isMine)
    {
        messageText.text = text;
        messageText.color = isMine ? ownerMessageColor : otherMessageColor;
    }

    public void SetName(string name, bool isMine)
    {
        playerNameText.text = name;
        playerNameText.color = isMine ? ownerMessageColor : otherMessageColor;
    }
}
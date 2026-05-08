using UnityEngine;

[CreateAssetMenu(menuName = "FeedbackTextSO")]
public class FeedbackTextSO : ScriptableObject
{
    [TextArea(3, 10)]
    public string lobbyStateText;

    [TextArea(3, 10)]
    public string micErrorText;
}
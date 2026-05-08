using UnityEngine;
using UnityEngine.EventSystems;

public class UIAudioHook : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private const string HOVER_SFX_ID = "UI HOVER";
    private const string CLICK_SFX_ID = "UI CLICK";
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(CLICK_SFX_ID))
            GlobalAudioManager.Instance?.PlayUISFX(CLICK_SFX_ID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(HOVER_SFX_ID))
            GlobalAudioManager.Instance?.PlayUISFX(HOVER_SFX_ID);
    }
}
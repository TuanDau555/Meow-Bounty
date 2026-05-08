using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUI : MonoBehaviour
{
    #region Parameters

    [SerializeField] private Transform uiRoot;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private Image progressImage;
    
    private Transform _playerCam;
    
    #endregion

    #region Execute

    private void Awake()
    {
        HideUI();
    }

    private void LateUpdate()
    {
        if(_playerCam == null || !uiRoot.gameObject.activeSelf) return;

        uiRoot.transform.LookAt(uiRoot.position + _playerCam.forward);
    }
    
    #endregion

    #region Show/Hide

    public void ShowUI(Transform playerCam)
    {
        _playerCam = playerCam;
        progressImage.fillAmount = 0;
        uiRoot.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        _playerCam = null;

        if (uiRoot != null)
            uiRoot.gameObject.SetActive(false);

        if (progressImage != null)
            progressImage.fillAmount = 0;
    }

    public void SetProgress(float progress)
    {
        if(progressImage == null) return;
        progressImage.fillAmount = Mathf.Clamp01(progress);
    }
    
    #endregion
}
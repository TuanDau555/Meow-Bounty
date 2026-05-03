using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingUI : MonoBehaviour
{
    #region KEYs

    private const string VSYNC_KEY = "vSync";
    private const string MOUSE_KEY = "mouseSen";
    
    #endregion

    #region Parameter
    
    [Header("Player Stats")]
    [SerializeField] private PlayerStatsSO playerStatsSO;

    [Header("UI")]
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Slider mouseSenSlider;

    public static event Action<float> OnMouseSensitiveChanged;
    public static event Action<bool> OnVSyncChanged;
    
    #endregion

    #region Execute

    private void Awake()
    {
        LoadSetting();
    }
    
    #endregion
    
    #region Setting Func

    public void OnVyncToggleChanged(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        Application.targetFrameRate = enabled ? 60 : -1;

        if(SceneManager.GetActiveScene().name != "Main Menu")
        {
            OnVSyncChanged?.Invoke(enabled);
        }
    }

    public void OnMouseSenChanged(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 10f); 
        
        PlayerPrefs.SetFloat(MOUSE_KEY, clamped);
        PlayerPrefs.Save();

        if(SceneManager.GetActiveScene().name != "Main Menu")
        {
            OnMouseSensitiveChanged?.Invoke(clamped);
        }
        
    }
    
    #endregion

    #region Load Setting

    private void LoadSetting()
    {
        LoadSingle(VSYNC_KEY, null, vSyncToggle);
        LoadSingle(MOUSE_KEY, mouseSenSlider, null);
    }
    
    private void LoadSingle(string key, Slider slider, Toggle toggle)
    {
        if(toggle != null && slider == null)
        {
            int vSync = PlayerPrefs.GetInt(VSYNC_KEY, 1); // default = ON
            QualitySettings.vSyncCount = vSync;
            Application.targetFrameRate = (vSync > 0) ? 60 : -1;
            vSyncToggle.SetIsOnWithoutNotify(vSync > 0);
        }

        if(slider != null && toggle == null)
        {
            float mouseSen = PlayerPrefs.GetFloat(MOUSE_KEY, 0.5f);
            slider.SetValueWithoutNotify(mouseSen);
            
            if(SceneManager.GetActiveScene().name != "Main Menu")
            {
                OnMouseSensitiveChanged?.Invoke(mouseSen);
            }
        }
    }
    
    #endregion
}
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// This class control music setting
/// </summary>
public class SoundMixerManager : MonoBehaviour
{
    #region Parameters
    
    // KEY for Save
    private const string MASTER_KEY = "MASTER_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";
    private const string MUSIC_KEY = "MUSIC_VOLUME";
    
    // Audio Mixer Parameters
    private const string MASTER_PARAMETER = "masterVolume";
    private const string SFX_PARAMETER = "sfxVolume";
    private const string MUSIC_PARAMETER = "musicVolume";

    [Header("UI Elements")]    
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    #endregion

    #region Excute

    private void Awake()
    {
        LoadVolume();
    }

    #endregion

    #region Control Sound

    public void SetMasterVolume(float level)
    {
        SetVolume(MASTER_PARAMETER, MASTER_KEY, level);
    }

    public void SetSfxVolume(float level)
    {
        SetVolume(SFX_PARAMETER, SFX_KEY, level);
    }

    public void SetMusicVolume(float level)
    {
        SetVolume(MUSIC_PARAMETER, MUSIC_KEY, level);
    }

    /// <summary>
    /// Set the value for volume
    /// </summary>
    /// <param name="parameter">Parameter from audio mixer</param>
    /// <param name="key">Save key</param>
    /// <param name="level">Sound Value</param>
    private void SetVolume(string parameter, string key, float level)
    {
        level = Mathf.Clamp(level, 0.0001f, 1f);
        
        // Because Unity using decibel for Audio Mixer, not the value between (0-1)
        // So we need to convert it to decible (clamp it)
        // You can search the formula on Google or Unity Doc
        float decibel = Mathf.Log10(level) * 20f;

        audioMixer.SetFloat(parameter, decibel);

        PlayerPrefs.SetFloat(key, level);
        PlayerPrefs.Save();
    }
    
    #endregion

    #region Load Setiing

    private void LoadVolume()
    {
        LoadSingle(MASTER_PARAMETER, MASTER_KEY, masterSlider);
        LoadSingle(SFX_PARAMETER, SFX_KEY, sfxSlider);
        LoadSingle(MUSIC_PARAMETER, MUSIC_KEY, musicSlider);
    }

    /// <summary>
    /// Get the save file
    /// </summary>
    /// <param name="parameter">Parameter from audio mixer</param>
    /// <param name="key">Save key</param>
    private void LoadSingle(string parameter, string key, Slider slider)
    {
        if(slider == null) return;

        float level = PlayerPrefs.GetFloat(key);
        float decibel = Mathf.Log10(level) * 20f;
        audioMixer.SetFloat(parameter, decibel);
        
        // Asign value to slider
        slider.SetValueWithoutNotify(level);
    }
    
    #endregion
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoHUD : MonoBehaviour 
{
    #region Parameters
    
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private float maxHealth;

    #endregion

    #region SET

    public void SetMaxHealth(float amount)
    {
        maxHealth = amount;
        healthSlider.maxValue = amount;
        healthText.text = amount.ToString();
    }

    public void SetHealth(float hp)
    {
        healthSlider.value = hp;
        healthText.text = hp.ToString();
    }
    
    #endregion
}
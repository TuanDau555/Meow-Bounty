using UnityEngine;
using UnityEngine.UI;

public class HitmakerUI : Singleton<HitmakerUI>
{
    #region Parameter
    
    [SerializeField] private Image hitmakerImage;
    [SerializeField] private float showTime = 0.15f;

    private float _timer = 0f;
    private bool _isShowing = false;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        hitmakerImage.enabled = false;  
    }

    private void Update()
    {
        ShowCounter();
    }

    #endregion

    #region Show

    public void ShowHitmaker()
    {
        hitmakerImage.enabled = true;
        _isShowing = true;
        _timer = showTime;
    }

    private void ShowCounter()
    {
        if(!_isShowing) return;

        _timer -= Time.deltaTime;
        
        if(_timer <= 0f)
        {
            HideHitmaker();
        }
    }
    
    private void HideHitmaker()
    {
        hitmakerImage.enabled = false;
        _isShowing = false;
        _timer = 0f;
    }
    
    #endregion
}

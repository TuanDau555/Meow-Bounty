using Cinemachine;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    #region Parameters
    [Header("Reference")]
    [SerializeField] private PlayerStatsSO playerStatsSO;
    [SerializeField] private CinemachineVirtualCamera vCam;

    [Header("RecoilS")]
    [SerializeField] private float recoilAmount = 2f;
    [SerializeField] private float recoilReturnSpeed = 5f;
    [SerializeField] private float recoilSnappiness = 10f;

    private CinemachinePOV _pov;
    private float _recoilTarget;
    private float _recoilCurrent;
    private float _previousRecoilCurrent;

    #endregion

    #region Execute

    private void Awake()
    {
        _pov = vCam.GetCinemachineComponent<CinemachinePOV>();
    }

    private void Update()
    {
        if(_pov == null)
        {
            Debug.LogError("POV in null");
            return;
        }

        _recoilTarget = Mathf.Lerp(
            _recoilTarget, 
            0f, 
            recoilReturnSpeed * Time.deltaTime
        );

        _previousRecoilCurrent = _recoilCurrent;

        _recoilCurrent = Mathf.Lerp(
            _recoilCurrent,
            _recoilTarget,
            recoilSnappiness * Time.deltaTime
        );
        
        float delta = _recoilCurrent - _previousRecoilCurrent;

        float limit = playerStatsSO.lookStats.lookLimit;
        
        _pov.m_VerticalAxis.Value  = Mathf.Clamp(
            _pov.m_VerticalAxis.Value + delta, 
            -limit, 
            limit
        );

    }

    #endregion

    #region Recoil

    public void AddRecoil()
    {
        _recoilTarget -= recoilAmount;
    }
    
    #endregion
}
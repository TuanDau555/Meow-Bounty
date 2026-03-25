using Cinemachine;
using UnityEngine;
public class CinemachinePovExtension : CinemachineExtension
{
    [SerializeField] private PlayerStatsSO playerStats;
    private Vector3 startingRotation;

    protected override void Awake()
    {
        base.Awake();
        
        startingRotation = transform.localRotation.eulerAngles;
    }
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (vcam.Follow)
        {
            if(stage == CinemachineCore.Stage.Aim)
            {        
                Vector2 _deltaInput = InputManager.Instance.GetMouseDelta();

                startingRotation.x += _deltaInput.x * playerStats.lookStats.lookSensitive * deltaTime;

                startingRotation.y += _deltaInput.y * playerStats.lookStats.lookSensitive * deltaTime;

                startingRotation.y = Mathf.Clamp(startingRotation.y, -playerStats.lookStats.lookLimit, playerStats.lookStats.lookLimit);

                state.RawOrientation = Quaternion.Euler(-startingRotation.y, startingRotation.z, 0f);
            }
        }
    }
}

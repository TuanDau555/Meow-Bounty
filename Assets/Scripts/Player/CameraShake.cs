using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public void Shake(float strength)
    {
        impulseSource.GenerateImpulse(strength);
    }
}
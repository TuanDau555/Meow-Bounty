using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// This script will be assigned in the vfx prefab
/// </summary>
public class FlameVFXController : MonoBehaviour 
{
    [SerializeField] private VisualEffect flameVFX;

    private bool _isPlaying = false;

    public void StartFlame()
    {
        if (_isPlaying) return;

        _isPlaying = true;
        flameVFX.Play();
    }

    public void StopFlame()
    {
        if (!_isPlaying) return;

        _isPlaying = false;
        flameVFX.Stop();
    }
    
}
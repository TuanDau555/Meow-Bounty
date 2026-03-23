using UnityEngine;

public interface ITargetable
{
    Transform GetTransform();
    bool IsValidTarget();
}
using UnityEngine;

public interface IPushable
{
    bool CanBePushed { get; }
    void ApplyPush(Vector3 pushDirection, float force);
}
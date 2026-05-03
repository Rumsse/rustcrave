using UnityEngine;

public interface IPushable
{
    void ApplyPush(Vector3 pushDirection, float force);
}
using UnityEngine;

public class HydraulicPressUnit : MonoBehaviour, IKillable
{
    [SerializeField] private SwarmState swarmState;

    private string currentUnitId;

    public void Initialize(string unitId)
    {
        currentUnitId = unitId;
    }

    public void Kill()
    {
        if (swarmState != null && !string.IsNullOrEmpty(currentUnitId))
        {
            swarmState.MarkDead(currentUnitId);
        }

        Destroy(gameObject);
    }
}

public interface IKillable
{
    void Kill();
}

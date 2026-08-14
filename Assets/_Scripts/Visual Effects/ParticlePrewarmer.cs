using UnityEngine;

public class ParticlePrewarmer : MonoBehaviour
{
    [SerializeField] private ParticleSystem targetParticleSystem;
    [SerializeField] private float simulateTime = 3f;

    private void OnEnable()
    {
        if (targetParticleSystem == null)
            return;

        targetParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        targetParticleSystem.Play(true);
        targetParticleSystem.Simulate(simulateTime, true, false);
    }
}
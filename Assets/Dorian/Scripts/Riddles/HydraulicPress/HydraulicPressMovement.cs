using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(Collider))]
public class HydraulicPressMovement : MonoBehaviour
{
    [SerializeField] private Transform pressTransform;
    [SerializeField] private Transform bottomPosition;
    [SerializeField] private float smashSpeed = 20f;
    [SerializeField] private ParticleSystem impactParticles;

    private Vector3 _targetBottomLocal;
    private bool _hasTriggered;

    private void Awake() => _targetBottomLocal = pressTransform.parent.InverseTransformPoint(bottomPosition.position);

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered)
            return;

        if (!other.CompareTag("Unit"))
            return;

        _hasTriggered = true;
        Debug.Log("Unit entered trigger. Executing one-time smash.");
        ExecuteSmash();
    }

    private void ExecuteSmash()
    {
        float distance = Vector3.Distance(pressTransform.localPosition, _targetBottomLocal);
        float smashDuration = distance / smashSpeed;

        Tween.LocalPosition(pressTransform, _targetBottomLocal, smashDuration, Ease.InCubic)
            .OnComplete(PlayImpactParticles);
    }

    private void PlayImpactParticles()
    {
        if (impactParticles != null)
            impactParticles.Play();
    }
}
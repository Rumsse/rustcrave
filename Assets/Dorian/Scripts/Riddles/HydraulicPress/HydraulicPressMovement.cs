using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider))]
public class HydraulicPressMovement : MonoBehaviour
{
    #region Inspector Fields

    [Header("Press Setup")]
    [SerializeField] private Transform pressTransform;
    [SerializeField] private MeshRenderer pressVisualRenderer;
    [SerializeField] private Transform bottomPosition;
    [SerializeField] private float smashSpeed = 20f;
    [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private ParticleSystem debrisParticles;

    [Header("Warning Shadow Setup")]
    [SerializeField] private DecalProjector shadowDecalProjector;
    [SerializeField] private float initialShadowFade = 0.3f;
    [SerializeField] private float maxShadowFade = 1f;
    [SerializeField] private float shadowFadeOutDuration = 0.5f;

    #endregion

    #region Private Fields

    private Vector3 _targetBottomLocal;
    private bool _hasTriggered;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _targetBottomLocal = pressTransform.parent.InverseTransformPoint(bottomPosition.position);
        SetupInitialShadowState();

        if (pressVisualRenderer)
            pressVisualRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered)
            return;

        if (!other.CompareTag("Unit"))
            return;

        _hasTriggered = true;
        Debug.Log("Unit entered trigger. Executing smash with dynamic shadow.");
        debrisParticles.Play();
        ExecuteSmash();
    }

    #endregion

    #region Logic

    private void SetupInitialShadowState()
    {
        if (!shadowDecalProjector)
            return;

        shadowDecalProjector.gameObject.SetActive(true);
        shadowDecalProjector.fadeFactor = initialShadowFade;
    }

    private void ExecuteSmash()
    {
        float distance = Vector3.Distance(pressTransform.localPosition, _targetBottomLocal);
        float smashDuration = distance / smashSpeed;

        if (shadowDecalProjector)
            Tween.Custom(shadowDecalProjector, initialShadowFade, maxShadowFade, smashDuration, SetDecalFade, Ease.InCubic);

        Tween.LocalPosition(pressTransform, _targetBottomLocal, smashDuration, Ease.InCubic)
            .OnComplete(HandleImpact);
    }

    private void HandleImpact()
    {
        if (impactParticles)
            impactParticles.Play();

        if (pressVisualRenderer)
            pressVisualRenderer.shadowCastingMode = ShadowCastingMode.On;

        if (shadowDecalProjector)
            Tween.Custom(shadowDecalProjector, maxShadowFade, 0f, shadowFadeOutDuration, SetDecalFade, Ease.InQuad)
                .OnComplete(HideDecal);
    }

    private void SetDecalFade(DecalProjector decal, float fade) => decal.fadeFactor = fade;

    private void HideDecal() => shadowDecalProjector.gameObject.SetActive(false);

    #endregion
}
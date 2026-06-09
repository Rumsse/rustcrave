using System;
using UnityEngine;
using PrimeTween;

public class OrePickUp : MonoBehaviour, IInteractable
{
    #region Events

    public static event Action OnAnyOrePickedUp;
    public event Action onInteract;

    #endregion

    #region Configuration

    public ItemSO item;
    public int oreValueAmount;

    [Header("Visual Effects")]
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private float hoverDuration = 1.5f;
    [SerializeField] private float hoverAmplitude = 0.25f;

    [Header("Drop Physics (Spawn)")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float jumpHeight = 1.5f;

    #endregion

    #region Unity Lifecycle

    private void OnDisable() => Tween.StopAll(transform);

    #endregion

    #region Drop Spawn Animation

    public void SpawnDrop(Vector3 sourcePosition, Vector3 targetPosition)
    {
        Tween.StopAll(transform);

        transform.position = sourcePosition;
        transform.localScale = Vector3.zero;

        Tween.Scale(transform, endValue: Vector3.one, duration: 0.3f, ease: Ease.OutBack);

        Tween.Custom(this, startValue: 0f, endValue: 1f, duration: dropDuration, ease: Ease.Linear, onValueChange: (target, t) =>
        {
            if (target == null)
                return;

            Vector3 currentPos = Vector3.Lerp(sourcePosition, targetPosition, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * target.jumpHeight;

            target.transform.position = currentPos;
        })
        .OnComplete(this, target => target.StartIdleAnimations());
    }

    #endregion

    #region Idle Animation Logic

    private void StartIdleAnimations()
    {
        Tween.LocalEulerAngles(transform, startValue: transform.localEulerAngles, endValue: transform.localEulerAngles + new Vector3(0f, 360f, 0f), duration: rotationDuration, ease: Ease.Linear, cycles: -1, cycleMode: CycleMode.Incremental);

        Tween.LocalPositionY(transform, startValue: transform.localPosition.y, endValue: transform.localPosition.y + hoverAmplitude, duration: hoverDuration, ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
    }

    #endregion

    #region Interaction

    public float InteractionTime => 0f;

    public void Interact()
    {
        onInteract?.Invoke();
        OnAnyOrePickedUp?.Invoke();
    }

    public void PlayEffect() { }

    public void StopEffect() { }

    #endregion
}
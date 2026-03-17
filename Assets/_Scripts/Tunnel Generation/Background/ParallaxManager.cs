using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ParallaxManager : MonoBehaviour
{
    public static ParallaxManager Instance { get; private set; }

    [SerializeField] private Transform cameraTransform;

    private readonly List<IParallaxLayer> layers = new List<IParallaxLayer>();
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Start() => lastCameraPosition = cameraTransform.position;

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        if (deltaMovement == Vector3.zero)
            return;

        foreach (var layer in layers)
            layer.MoveParallax(deltaMovement, cameraTransform.position);

        lastCameraPosition = cameraTransform.position;
    }

    public void RegisterLayer(IParallaxLayer layer)
    {
        if (!layers.Contains(layer))
            layers.Add(layer);
    }

    public void UnregisterLayer(IParallaxLayer layer)
    {
        if (layers.Contains(layer))
            layers.Remove(layer);
    }
}
using UnityEngine;

public class CommandVisualizer : MonoBehaviour
{
    [Header("Drag Visuals")]
    [SerializeField] private LineRenderer commandLineRenderer;
    [SerializeField] private GameObject dragStartIndicatorPrefab;
    [SerializeField] private GameObject dragTargetIndicatorPrefab;
    [SerializeField] private float indicatorYOffset = 0.05f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private LayerMask oreLayerMask;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask mouseWorldLayerMask;

    private GameObject dragStartIndicator;
    private GameObject dragTargetIndicator;

    private void Awake()
    {
        if (dragStartIndicatorPrefab != null)
        {
            dragStartIndicator = Instantiate(dragStartIndicatorPrefab);
            dragStartIndicator.SetActive(false);
        }

        if (dragTargetIndicatorPrefab != null)
        {
            dragTargetIndicator = Instantiate(dragTargetIndicatorPrefab);
            dragTargetIndicator.SetActive(false);
        }

        if (commandLineRenderer != null)
        {
            commandLineRenderer.enabled = false;
        }
    }

    public void StartVisuals(Unit unit)
    {
        if (Time.timeScale == 0)
            return;

        if (dragStartIndicator != null)
        {
            dragStartIndicator.transform.position = unit.transform.position + new Vector3(unit.transform.position.x, indicatorYOffset, unit.transform.position.z);
            dragStartIndicator.SetActive(true);
        }

        if (dragTargetIndicator != null)
        {
            dragTargetIndicator.SetActive(true);
        }

        if (commandLineRenderer != null)
        {
            commandLineRenderer.enabled = true;
        }

        UpdateVisuals(unit);
    }

    public void UpdateVisuals(Unit unit)
    {
        Vector3 startPos = unit.transform.position + new Vector3(0f, indicatorYOffset, 0f);
        Vector3 targetPos = startPos;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask combinedMask = interactableLayerMask | oreLayerMask | unitLayerMask;

        if (Physics.Raycast(ray, out RaycastHit interactableHit, float.MaxValue, combinedMask))
        {
            targetPos = interactableHit.collider.transform.position + new Vector3(0f, indicatorYOffset, 0f);
        }
        else if (Physics.Raycast(ray, out RaycastHit groundHit, float.MaxValue, mouseWorldLayerMask))
        {
            targetPos = groundHit.point + new Vector3(0f, indicatorYOffset, 0f);
        }

        if (commandLineRenderer != null)
        {
            commandLineRenderer.SetPosition(0, startPos);
            commandLineRenderer.SetPosition(1, targetPos);
        }

        if (dragTargetIndicator != null)
        {
            dragTargetIndicator.transform.position = targetPos;
        }

        if (dragStartIndicator != null)
        {
            dragStartIndicator.transform.position = startPos;
        }
    }

    public void StopVisuals()
    {
        if (dragStartIndicator != null) dragStartIndicator.SetActive(false);
        if (dragTargetIndicator != null) dragTargetIndicator.SetActive(false);
        if (commandLineRenderer != null) commandLineRenderer.enabled = false;
    }
}
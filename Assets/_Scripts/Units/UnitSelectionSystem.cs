using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionSystem : MonoBehaviour
{
    #region Properties & Fields

    public static UnitSelectionSystem Instance { get; private set; }

    public event EventHandler OnSelectedUnitsChanged;

    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private LayerMask unitLayerMask;

    [Header("Box Visuals")]
    [SerializeField] private Color fillColor = new Color(0.2f, 0.8f, 0.2f, 0.2f);
    [SerializeField] private Color borderColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private float borderThickness = 2f;

    private List<Unit> selectedUnits = new();
    private Vector2 startMousePosition;

    private const float MIN_DRAG_DISTANCE = 10f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;
        SetupSelectionVisuals();
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        HandleSelectionInput();
    }

    #endregion

    #region Visual Setup

    private void SetupSelectionVisuals()
    {
        if (selectionBox.TryGetComponent(out Image img))
            Destroy(img);

        foreach (Transform child in selectionBox)
            Destroy(child.gameObject);

        CreateFill();
        CreateBorder("Top", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -borderThickness / 2f), new Vector2(0, borderThickness));
        CreateBorder("Bottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, borderThickness / 2f), new Vector2(0, borderThickness));
        CreateBorder("Left", new Vector2(0, 0), new Vector2(0, 1), new Vector2(borderThickness / 2f, 0), new Vector2(borderThickness, 0));
        CreateBorder("Right", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-borderThickness / 2f, 0), new Vector2(borderThickness, 0));
    }

    private void CreateFill()
    {
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(selectionBox, false);

        Image image = fillObj.AddComponent<Image>();
        image.color = fillColor;
        image.raycastTarget = false;

        RectTransform rect = fillObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private void CreateBorder(string edgeName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject edge = new GameObject(edgeName);
        edge.transform.SetParent(selectionBox, false);

        Image image = edge.AddComponent<Image>();
        image.color = borderColor;
        image.raycastTarget = false;

        RectTransform rect = edge.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
    }

    #endregion

    #region Selection Logic

    private void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
            UpdateSelectionBox();
        }

        if (Input.GetMouseButton(0))
            UpdateSelectionBox();

        if (!Input.GetMouseButtonUp(0))
            return;

        selectionBox.gameObject.SetActive(false);
        ProcessSelection();
    }

    private void UpdateSelectionBox()
    {
        RectTransform parentRect = (RectTransform)selectionBox.parent;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, startMousePosition, null, out Vector2 localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, null, out Vector2 localEnd);

        Vector2 lowerLeft = Vector2.Min(localStart, localEnd);
        Vector2 upperRight = Vector2.Max(localStart, localEnd);

        selectionBox.anchoredPosition = lowerLeft + (upperRight - lowerLeft) / 2f;
        selectionBox.sizeDelta = upperRight - lowerLeft;
    }

    private void ProcessSelection()
    {
        ClearSelection();

        if (Vector2.Distance(startMousePosition, Input.mousePosition) < MIN_DRAG_DISTANCE)
            TrySelectSingleUnit();
        else
            SelectUnitsInBox();
    }

    private void TrySelectSingleUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, unitLayerMask))
            return;

        if (!hit.transform.TryGetComponent(out Unit unit))
            return;

        AddUnitToSelection(unit);
        NotifySelectionChanged();
    }

    private void SelectUnitsInBox()
    {
        Vector2 min = Vector2.Min(startMousePosition, Input.mousePosition);
        Vector2 max = Vector2.Max(startMousePosition, Input.mousePosition);

        foreach (Unit unit in Unit.units)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
                AddUnitToSelection(unit);
        }

        NotifySelectionChanged();
    }

    private void AddUnitToSelection(Unit unit)
    {
        if (selectedUnits.Contains(unit))
            return;

        selectedUnits.Add(unit);
        unit.PlaySelectSound();

        if (unit.IsMainCharacter)
            return;

        Unit.MainCharacter?.PlayCommandAnimation();
    }

    public void ClearSelection()
    {
        selectedUnits.Clear();
        NotifySelectionChanged();
    }

    public void SetSelectedUnit(Unit unit)
    {
        ClearSelection();

        if (unit == null)
            return;

        AddUnitToSelection(unit);
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged() => OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);

    #endregion

    #region Public Accessors

    public List<Unit> GetSelectedUnits() => selectedUnits;

    public Unit GetPrimarySelectedUnit() => selectedUnits.Count > 0 ? selectedUnits[0] : null;

    #endregion
}
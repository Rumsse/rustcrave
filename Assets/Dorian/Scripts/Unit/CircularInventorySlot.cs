using UnityEngine;
using UnityEngine.UI;

public class CircularInventorySlot : MonoBehaviour
{

    #region Inspector Fields

    [SerializeField] private Image fillImage;
    [SerializeField] private Color emptyColor = new Color(0, 0, 0, 0f);

    #endregion

    #region Unity Lifecycle

    private void Awake() => ValidateComponents();

    #endregion

    #region Setup

    public void Setup(float fillAmount, float rotationAngle)
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = fillAmount;
        transform.localEulerAngles = new Vector3(0, 0, -rotationAngle);
        SetEmpty();
    }

    public void SetItem(Color itemColor)
    {
        if (fillImage == null)
            return;

        fillImage.color = itemColor;
    }

    public void SetEmpty()
    {
        if (fillImage == null)
            return;

        fillImage.color = emptyColor;
    }

    #endregion

    #region Validation

    private void ValidateComponents()
    {
        if (fillImage == null)
            Debug.LogWarning("Missing fillImage on CircularInventorySlot");
    }

    #endregion

}
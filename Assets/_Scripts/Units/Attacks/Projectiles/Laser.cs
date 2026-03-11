using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Laser : MonoBehaviour
{
    public Transform Visuals => _visuals;
    public DecalProjector Indicator => _indicator;
    
    public Vector3 InitialLaserScale => _initialLaserScale;
    
    [Header("References")]
    [SerializeField] private Transform _visuals;
    [SerializeField] private DecalProjector _indicator;
    
    [Header("Scale Settings")]
    [SerializeField] private bool _scaleX;
    [SerializeField] private bool _scaleY;
    [SerializeField] private bool _scaleZ;
    
    [Header("Tween Settings (Backup)")]
    [Tooltip("This is backup settings. Attack should have its own settings in SO.")]
    [SerializeField] private TweenSettings _scaleSettings;

    private Vector3 _initialIndicatorScale;
    private Vector3 _initialLaserScale;

    private void Awake()
    {
        _initialIndicatorScale = _indicator.size;
        _initialLaserScale = _visuals.localScale;
    }
    
    /// <param name="literal">If false it sets value based on initial Scale</param>
    public void SetVisualsScale(float scale, TweenSettings? settings = null, bool literal = false)
    {
        scale = literal ? scale : _initialLaserScale.x * scale;
        var endValue = GetVisualsScale(scale);
        
        Tween.Scale(_visuals, new TweenSettings<Vector3>(endValue, settings ?? _scaleSettings));
    }

    /// <param name="literal">If false it sets value based on initial Scale</param>
    public void SetVisualsScaleInstant(float scale, bool literal = false)
    {
        scale = literal ? scale : _initialLaserScale.x * scale;
        
        _visuals.localScale = GetVisualsScale(scale);
    }

    private Vector3 GetVisualsScale(float scale)
    {
        float x = _scaleX ? scale : _visuals.localScale.x;
        float y = _scaleY ? scale : _visuals.localScale.y;
        float z = _scaleZ ? scale : _visuals.localScale.z;
        
        return new Vector3(x, y, z);
    }

    /// <param name="literal">If false it sets value based on initial Width</param>
    public void SetIndicatorWidth(float width, TweenSettings? settings = null, bool literal = false)
    {
        float targetWidth = literal ? width : _initialIndicatorScale.x * width;
    
        Tween.Custom(
            _indicator.size.x,
            targetWidth,
            settings ?? _scaleSettings,
            onValueChange: val => {
                var size = _indicator.size;
                size.x = val;
                _indicator.size = size;
            });
    }

    /// <param name="literal">If false it sets value based on initial Width</param>
    public void SetIndicatorWidthInstant(float width, bool literal = false)
    {
        width = literal ? width : _initialIndicatorScale.x * width;
        
        var size = _indicator.size;
        size.x = width;
        
        _indicator.size = size;
    }

    /// <param name="literal">If false it sets value based on initial Height</param>
    public void SetIndicatorHeight(float height, TweenSettings? settings = null, bool literal = false)
    {
        float targetHeight = literal ? height : _initialIndicatorScale.y * height;

        Tween.Custom(
            _indicator.size.y,
            targetHeight,
            settings ?? _scaleSettings,
            onValueChange: val => {
                var size = _indicator.size;
                size.y = val;
                _indicator.size = size;
            });
    }

    /// <param name="literal">If false it sets value based on initial Height</param>
    public void SetIndicatorHeightInstant(float height, bool literal = false)
    {
        height = literal ? height : _initialIndicatorScale.y * height;
        
        var size = _indicator.size;
        size.y = height;
        
        _indicator.size = size;
    }
}

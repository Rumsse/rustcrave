using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public sealed class OffscreenKillZone : MonoBehaviour
{
    #region Inspector

    [SerializeField] float margin = 0.5f;
    [SerializeField] float killEnableDelay = 0.25f;

    #endregion

    #region Private

    const float PerspectiveDepth = 10f;

    Camera _cam;
    BoxCollider _col;
    Rigidbody _rb;
    bool _isKillEnabled;
    float _killEnableAt;

    #endregion

    #region Unity

    void Awake()
    {
        _cam = Camera.main;
        _col = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _col.isTrigger = true;
    }

    void OnEnable()
    {
        _isKillEnabled = false;
        _killEnableAt = Time.time + killEnableDelay;
    }

    void LateUpdate()
    {
        UpdatePlacement();
        EnableKillIfReady();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_isKillEnabled) return;
        if (!other.TryGetComponent<HealthManager>(out var health)) return;
        health.Death();
    }

    #endregion

    #region Internal

    void UpdatePlacement()
    {
        if (!_cam) return;

        if (_cam.orthographic)
        {
            var h = _cam.orthographicSize * 2f;
            var w = h * _cam.aspect;

            _col.size = new Vector3(margin, h * 1.2f, 100f);
            transform.SetPositionAndRotation(
                _cam.transform.position - _cam.transform.right * (w * 0.5f + margin),
                _cam.transform.rotation
            );
            return;
        }

        var hP = Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * PerspectiveDepth * 2f;
        var wP = hP * _cam.aspect;

        _col.size = new Vector3(margin, hP * 3f, 500f);
        transform.SetPositionAndRotation(
            _cam.transform.position - _cam.transform.right * (wP * 0.5f + margin),
            _cam.transform.rotation
        );
    }

    void EnableKillIfReady()
    {
        if (_isKillEnabled) return;
        if (Time.time < _killEnableAt) return;
        _isKillEnabled = true;
    }

    #endregion
}
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.AI;

public class BossCoreP2Spawner : MonoBehaviour
{

    #region Serialized Fields

    [Header("Phase 2 Spawner")]
    [SerializeField] private Transform _unitTransform;
    [SerializeField] private Collider _col;
    [SerializeField] private Transform _startPos;
    [SerializeField] private float _jumpHeight = 1f;
    [SerializeField] private List<Transform> _possibleEndPositions = new();
    [SerializeField] private TweenSettings _positionSettings;
    [SerializeField] private int _phaseIndex;

    [Header("Boss Death Transition")]
    [SerializeField] private ParticleSystem _explosionEffect;
    [SerializeField] private ParticleSystem _flamesEffect;
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private GameObject _bossPrefabFallen;
    [SerializeField] private Renderer _bossRenderer;
    [SerializeField] private Material _dissolveMaterial;
    [SerializeField] private string _texturePropertyName = "_MainTex";
    [SerializeField] private string _dissolvePropertyName = "_DissolveAmount";
    [SerializeField] private float _deathDelay = 1f;
    [SerializeField] private float _deathDuration = 1.5f;
    [SerializeField] private float _scaleYTarget = 0.5f;
    [SerializeField] private float _positionYOffset = -2f;
    [SerializeField] private float _dissolveStartValue = 0f;
    [SerializeField] private float _dissolveEndValue = 1.2f;

    [Header("Unit")]
    [SerializeField] private UnitSO _unit;

    #endregion

    #region Private Fields

    private bool _isExecuted;

    #endregion

    #region Methods

    public void Execute(int phaseIndex)
    {
        Debug.Log($"BossCoreP2Spawner received Execute call for phase index: {phaseIndex}. Current execution state: {_isExecuted}");

        if (phaseIndex != _phaseIndex || _isExecuted)
            return;

        _isExecuted = true;

        if (_bossRenderer == null || _dissolveMaterial == null)
        {
            Debug.LogError("Missing references for boss dissolve transition.");
            return;
        }

        Transform bossTransform = _bossPrefab.transform;

        Sequence.Create()
            .ChainDelay(_deathDelay)
            .ChainCallback(() => PrepareDeathEffects())
            .Group(Tween.Custom(_bossRenderer, _dissolveStartValue, _dissolveEndValue, _deathDuration, (r, val) => r.material.SetFloat(_dissolvePropertyName, val)))
            .Group(Tween.ScaleY(bossTransform, _scaleYTarget, _deathDuration))
            .Group(Tween.PositionY(bossTransform, bossTransform.position.y + _positionYOffset, _deathDuration))
            .OnComplete(() =>
            {
                //_bossPrefab.SetActive(false);
                //_bossPrefabFallen.SetActive(true);
                SpawnPhaseTwoUnit();
            });
    }

    private void PrepareDeathEffects()
    {
        _explosionEffect.Play();
        _flamesEffect.Play();

        Material instanceDissolveMat = new Material(_dissolveMaterial);
        Texture mainTex = _bossRenderer.material.GetTexture(_texturePropertyName);

        if (mainTex != null)
            instanceDissolveMat.SetTexture(_texturePropertyName, mainTex);

        _bossRenderer.material = instanceDissolveMat;
    }

    private void SpawnPhaseTwoUnit()
    {
        Vector3 targetPos = _possibleEndPositions[Random.Range(0, _possibleEndPositions.Count)].position;

        var unit = _unitTransform;
        _col.enabled = false;
        unit.gameObject.SetActive(true);

        var agent = unit.GetComponentInChildren<NavMeshAgent>();
        agent.enabled = false;

        unit.position = _startPos.position;

        Tween.PositionX(unit, new TweenSettings<float>(targetPos.x, _positionSettings));
        Tween.PositionZ(unit, new TweenSettings<float>(targetPos.z, _positionSettings));

        TweenSettings yPosSettings = _positionSettings;
        yPosSettings.duration *= .5f;
        Tween.PositionY(unit, new TweenSettings<float>(_jumpHeight, yPosSettings))
            .Chain(Tween.PositionY(unit, new TweenSettings<float>(targetPos.y, yPosSettings)))
            .OnComplete(() =>
            {
                agent.enabled = true;
                _col.enabled = true;
            });
    }

    #endregion

}
using System.Collections;
using UnityEngine;

public class MimicryEnemy : EnemyUnit
{
    [SerializeField] private GameObject mimicryObject;
    [SerializeField] private GameObject mimicryVisualObject;
    [SerializeField] private float offset = 1.5f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float magnitude = 0.001f;

    private Vector3 _startLocalPos;
    private WaitForSeconds _waitOffset;
    private bool _isMimicking = true;

    protected override void Start()
    {
        base.Start();

        _startLocalPos = mimicryObject.transform.localPosition;
        _waitOffset = new WaitForSeconds(offset);
        StartCoroutine(ShakeCoroutine());
    }

    public override void PrepareUnit()
    {
        if (!_isMimicking) return;

        StopAllCoroutines();
        mimicryObject.SetActive(false);
        mimicryVisualObject.SetActive(true);
        _isMimicking = false;

        selectedVisualObject.SetActive(true);
        animator.Play(specialAnimationName);//?
    }

    #region Coroutines

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = _startLocalPos + new Vector3(x, y, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _startLocalPos;

        StartCoroutine(MimicryCoroutine());
    }

    private IEnumerator MimicryCoroutine()
    {
        yield return _waitOffset;

        StartCoroutine(ShakeCoroutine());
    }

    #endregion
}

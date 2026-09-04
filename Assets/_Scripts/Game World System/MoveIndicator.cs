using UnityEngine;

public class MoveIndicator : MonoBehaviour
{
    public int DefaultPoolCapacity => defaultPoolCapacity;
    public int MaxPoolSize => maxPoolSize;

    [SerializeField] private float lifetime;
    [SerializeField] private int defaultPoolCapacity = 5;
    [SerializeField] private int maxPoolSize = 30;

    private MoveIndicator prefabReference;
    private float timer;

    public void Initialize(MoveIndicator prefab)
    {
        prefabReference = prefab;
        timer = lifetime;
    }

    private void Update()
    {
        if (timer <= 0f)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            PoolManager.Instance.Release(this, prefabReference);
    }
}
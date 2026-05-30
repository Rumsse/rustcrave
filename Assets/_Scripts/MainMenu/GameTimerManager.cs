using UnityEngine;

public class GameTimerManager : MonoBehaviour
{
    public static GameTimerManager Instance { get; private set; }

    public float TotalPlayTime { get; private set; }
    private bool isTimerRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!isTimerRunning)
            return;

        TotalPlayTime += Time.deltaTime;
    }

    #region Public API

    public void StartTimer() => isTimerRunning = true;

    public void StopTimer() => isTimerRunning = false;

    public void ResetTimer()
    {
        TotalPlayTime = 0f;
        isTimerRunning = false;
    }

    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(TotalPlayTime / 3600f);
        int minutes = Mathf.FloorToInt((TotalPlayTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(TotalPlayTime % 60f);

        if (hours > 0)
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #endregion
}
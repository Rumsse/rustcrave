using UnityEngine;

public class GameTimerManager : MonoBehaviour
{
    public static GameTimerManager Instance { get; private set; }

    [SerializeField] bool showDebugTimer;

    public float TotalPlayTime { get; private set; }
    bool isTimerRunning;

    #region Unity Lifecycle

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!isTimerRunning)
            return;

        TotalPlayTime += Time.deltaTime;
    }

    void OnGUI()
    {
        if (!showDebugTimer)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.yellow;

        GUI.Label(new Rect(20, 20, 400, 50), $"{GetFormattedTime()}", style);
    }

    #endregion

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

    #region Save System

    public float GetSaveData() => TotalPlayTime;

    public void LoadFromSave(float savedTime) => TotalPlayTime = savedTime;

    #endregion
}
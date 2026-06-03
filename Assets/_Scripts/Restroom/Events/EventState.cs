using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EventState", menuName = "Save System/EventState")]
public class EventState : ScriptableObject
{
    public event Action OnStateLoaded;

    [HideInInspector] public bool isResolved;
    [HideInInspector] public string currentEventName;

    public void Reset()
    {
        isResolved = false;
        currentEventName = string.Empty;
    }

    public EventSaveData GetSaveData() => new EventSaveData { isResolved = isResolved, currentEventName = currentEventName };

    public void LoadFromSave(EventSaveData data)
    {
        isResolved = data.isResolved;
        currentEventName = data.currentEventName;

        OnStateLoaded?.Invoke();
    }
}

[Serializable]
public class EventSaveData
{
    public bool isResolved;
    public string currentEventName;
}
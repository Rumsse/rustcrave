using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogOption
{
    public string optionText;
    public bool isIgnoreOption;

    [SerializeReference, EventActionPicker]
    public EventCheck check = new LuckCheck();

    public EventOutcome successOutcome;
    public EventOutcome failureOutcome;

    [SerializeReference, EventActionPicker]
    public List<EventRequirement> requirements = new();
}

[Serializable]
public class EventOutcome
{
    [TextArea(3, 5)] public string resultText;
    public bool keepEventActive;

    [SerializeReference, EventActionPicker]
    public List<EventAction> actions = new();
}

[CreateAssetMenu(fileName = "NewRandomEvent", menuName = "Restroom/Events/Random Event")]
public class RandomEvent : ScriptableObject
{
    public string eventTitle;
    public Sprite eventIcon;
    [TextArea(4, 6)] public string eventDescription;
    public List<DialogOption> dialogOptions = new();
}

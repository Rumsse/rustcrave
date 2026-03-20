using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventOutcome
{
    [TextArea(3, 5)] public string resultText;
    public List<EventAction> actions = new();
}

[Serializable]
public class DialogOption
{
    public string optionText;
    public bool isIgnoreOption;

    public bool isMiningCheck;
    [Range(0, 100)] public int baseSuccessChance = 50;
    public int bonusPerMiningPower = 10;

    public EventOutcome successOutcome;
    public EventOutcome failureOutcome;
}

[CreateAssetMenu(fileName = "NewRandomEvent", menuName = "Restroom/Events/Random Event")]
public class RandomEvent : ScriptableObject
{
    public string eventTitle;
    [TextArea(4, 6)] public string eventDescription;
    public List<DialogOption> dialogOptions = new();
}
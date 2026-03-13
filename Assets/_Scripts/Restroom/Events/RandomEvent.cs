using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogOption
{
    public string optionText;
    public EventAction action;
}

[CreateAssetMenu(fileName = "NewRandomEvent", menuName = "Restroom/Events/Random Event")]
public class RandomEvent : ScriptableObject
{
    public string eventTitle;
    [TextArea(4, 6)] public string eventDescription;
    public List<DialogOption> dialogOptions = new();
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventDatabase", menuName = "Restroom/Events/Event Database")]
public class EventDatabase : ScriptableObject
{
    public List<RandomEvent> availableEvents = new();
}
using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public SwarmSaveData swarmData = new();
    public InventorySaveData inventoryData = new();
    public GadgetsSaveData gadgetsData = new();
    public MapSaveData mapData = new();
    public EventSaveData eventData = new();

    public float totalPlayTime;
}

[Serializable]
public class SwarmSaveData
{
    public List<UnitSaveData> units = new();
}

[Serializable]
public class UnitSaveData
{
    public string id;
    public string unitTypeName;
    public int currentHP;
    public float currentEnergy;
    public bool isAlive;
    public List<string> assignedGadgetNames = new();
}

[Serializable]
public class InventorySaveData
{
    public List<SlotSaveData> slots = new();
}

[Serializable]
public class SlotSaveData
{
    public string itemName;
    public int amount;
}

[Serializable]
public class GadgetsSaveData
{
    public List<string> unlockedGadgetNames = new();
}

[Serializable]
public class MapSaveData
{
    public List<PathNodeData> nodes = new();
    public List<string> scannedNodeIds = new();
    public List<string> visitedNodeIds = new();
    public string currentNodeId;
}
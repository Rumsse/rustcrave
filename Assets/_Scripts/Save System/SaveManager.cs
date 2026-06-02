using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private GameDatabase database;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private GlobalInventorySO globalInventory;
    [SerializeField] private GadgetsGlobalInventory gadgetsInventory;
    [SerializeField] private MapState mapState;

    private string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.json");

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

    public void SaveGame()
    {
        var data = new GameSaveData();

        data.swarmData = swarmState.GetSaveData();
        data.inventoryData = globalInventory.GetSaveData();
        data.gadgetsData = gadgetsInventory.GetSaveData();
        data.mapData = mapState.GetSaveData();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Game saved successfully at: {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
            return;

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<GameSaveData>(json);

        if (data == null)
            return;

        swarmState.LoadFromSave(data.swarmData, database);
        globalInventory.LoadFromSave(data.inventoryData, database);
        gadgetsInventory.LoadFromSave(data.gadgetsData, database);
        mapState.LoadFromSave(data.mapData);

        Debug.Log("Game loaded successfully.");
    }

    public void DeleteSave()
    {
        if (!File.Exists(SavePath))
            return;

        File.Delete(SavePath);
        Debug.Log("Save file deleted.");
    }

    public bool HasSaveFile() => File.Exists(SavePath);
}
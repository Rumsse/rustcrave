using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static event Action OnGameSaved;
    public static SaveManager Instance { get; private set; }

    [SerializeField] bool useEncryption = true;
    [SerializeField] GameDatabase database;
    [SerializeField] SwarmState swarmState;
    [SerializeField] GlobalInventorySO globalInventory;
    [SerializeField] GadgetsGlobalInventory gadgetsInventory;
    [SerializeField] MapState mapState;
    [SerializeField] EventState eventState;

    readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("8x/A?D(G+KbPeShV");
    readonly byte[] encryptionIV = Encoding.UTF8.GetBytes("F-JaNdRgUkXp2s5v");

    public int CurrentSlot { get; private set; } = 1;

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

    #endregion

    #region Slots & Meta API

    string GetSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"gamesave_slot_{slot}.sav");

    public bool HasSaveFile(int slot) => File.Exists(GetSavePath(slot));

    public bool HasAnySave() => HasSaveFile(0) || HasSaveFile(1) || HasSaveFile(2) || HasSaveFile(3);

    public string GetSlotDate(int slot)
    {
        if (!HasSaveFile(slot))
            return "Empty Slot";

        return PlayerPrefs.GetString($"Slot_{slot}_Date", "Unknown Date");
    }

    public void SetCurrentSlot(int slot)
    {
        CurrentSlot = slot;
        PlayerPrefs.SetInt("LastPlayedSlot", slot);
        PlayerPrefs.Save();
    }

    #endregion

    #region Main Save / Load API

    public void AutoSaveGame() => PerformSave(0);

    public void SaveGame()
    {
        if (CurrentSlot == 0)
        {
            Debug.LogWarning("[SaveManager] Cannot manually save to Autosave slot. Choose a manual slot.");
            return;
        }

        PerformSave(CurrentSlot);
    }

    void PerformSave(int slot)
    {
        var data = new GameSaveData();

        data.swarmData = swarmState.GetSaveData();
        data.inventoryData = globalInventory.GetSaveData();
        data.gadgetsData = gadgetsInventory.GetSaveData();
        data.mapData = mapState.GetSaveData();
        data.eventData = eventState.GetSaveData();

        if (GameTimerManager.Instance != null)
            data.totalPlayTime = GameTimerManager.Instance.GetSaveData();

        string json = JsonUtility.ToJson(data, true);
        string finalData = useEncryption ? Encrypt(json) : json;

        File.WriteAllText(GetSavePath(slot), finalData);

        PlayerPrefs.SetString($"Slot_{slot}_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        PlayerPrefs.SetInt("LastPlayedSlot", slot);
        PlayerPrefs.Save();

        Debug.Log($"[SaveManager] Game saved successfully to Slot {slot}.");
        OnGameSaved?.Invoke();
    }

    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogError($"[SaveManager] No save file in Slot {slot}!");
            return;
        }

        SetCurrentSlot(slot);

        string rawData = File.ReadAllText(path);
        string json = useEncryption ? Decrypt(rawData) : rawData;

        var data = JsonUtility.FromJson<GameSaveData>(json);

        if (data == null)
        {
            Debug.LogError("[SaveManager] Failed to parse save data!");
            return;
        }

        swarmState.LoadFromSave(data.swarmData, database);
        globalInventory.LoadFromSave(data.inventoryData, database);
        gadgetsInventory.LoadFromSave(data.gadgetsData, database);
        mapState.LoadFromSave(data.mapData);
        eventState.LoadFromSave(data.eventData);

        if (GameTimerManager.Instance != null)
            GameTimerManager.Instance.LoadFromSave(data.totalPlayTime);

        Debug.Log($"[SaveManager] Loaded Slot {slot} successfully.");
    }

    public void ContinueGame()
    {
        int lastSlot = PlayerPrefs.GetInt("LastPlayedSlot", 0);
        LoadGame(lastSlot);
    }

    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
            return;

        File.Delete(path);
        PlayerPrefs.DeleteKey($"Slot_{slot}_Date");
        PlayerPrefs.Save();

        Debug.Log($"[SaveManager] Save slot {slot} deleted.");
    }

    #endregion

    #region Encryption Logic

    string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = encryptionKey;
        aes.IV = encryptionIV;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);

        sw.Write(plainText);
        sw.Close();

        return Convert.ToBase64String(ms.ToArray());
    }

    string Decrypt(string cipherText)
    {
        using Aes aes = Aes.Create();
        aes.Key = encryptionKey;
        aes.IV = encryptionIV;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var buffer = Convert.FromBase64String(cipherText);

        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    #endregion
}
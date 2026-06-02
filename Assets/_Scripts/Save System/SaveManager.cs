using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
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

    string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.sav");

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

    #region Public API

    public void SaveGame()
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

        File.WriteAllText(SavePath, finalData);

        Debug.Log($"[SaveManager] Game saved successfully at: {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
            return;

        string rawData = File.ReadAllText(SavePath);
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

        Debug.Log("[SaveManager] Game loaded successfully.");
    }

    public void DeleteSave()
    {
        if (!File.Exists(SavePath))
            return;

        File.Delete(SavePath);
        Debug.Log("[SaveManager] Save file deleted.");
    }

    public bool HasSaveFile() => File.Exists(SavePath);

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
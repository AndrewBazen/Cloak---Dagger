
using System.Collections.Generic;
using System.IO;
using Start.Resources;
using Start.Scripts.Game;
using Start.Scripts.Character;
using Start.Scripts.Enemy;
using Start.Scripts.Levels;
using Newtonsoft.Json;
using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private List<SaveData> _saves;
    private GameManager _gameManager;

    private GameObject _enemyPrefab;
    private GameObject _playerPrefab;
    private GameObject _enemyContainer;
    private GameObject _playerContainer;

    private string GameSavePath => Application.persistentDataPath + "/GameSaves";

    public List<SaveData> Saves
    {
        get => _saves;
        set
        {
            _saves = value;
        }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _gameManager = GameManager.Instance;
        Initialize();
    }

    private void Initialize()
    {
        // Ensure the save directory exists
        if (!Directory.Exists(GameSavePath))
        {
            Directory.CreateDirectory(GameSavePath);
        }
        _enemyPrefab = _gameManager.EnemyPrefab;
        _playerPrefab = _gameManager.PlayerPrefab;
        _enemyContainer = _gameManager.EnemyContainer;
        _playerContainer = _gameManager.PlayerContainer;
    }

    public void SaveGame(string saveName)
    {
        SaveData saveData = new SaveData();

        // ensure party data is up to date
        _gameManager.Party.UpdatePartyData();
        foreach (var player in _gameManager.Party.Party)
        {   

            saveData.Party.Add(player.characterData);
        }

        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(GetSaveFilePath(saveName), json);
        Debug.Log("Saved to " + GetSaveFilePath(saveName));
    }

    public void LoadGame(string saveName)
    {
        // check if the save file exists
        if (!File.Exists(GetSaveFilePath(saveName)))
        {
            Debug.LogWarning("No save file found at " + GetSaveFilePath(saveName));
            return;
        }

        // read the save file
        string json = File.ReadAllText(GetSaveFilePath(saveName));
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
        // load the levelData
        _gameManager.SetCurrentLevelData(saveData.CurrentLevel);
        if (_gameManager.CurrentLevelData == null)
        {
            Debug.LogError("Unable to load level. Data not found or corrupted.");
            return;
        }
        // load the enemeies
        List<EnemyData> loadedEnemies = new List<EnemyData>();
        foreach (var enemyData in saveData.Enemies)
        {
            GameObject enemy = Instantiate(_gameManager.EnemyPrefab, enemyData.Position, Quaternion.identity, _gameManager.EnemyContainer.transform);
            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(enemyData);
                loadedEnemies.Add(enemyData);
            }
            else
            {
                Debug.LogError("EnemyController not found on prefab.");
            }
        }
        if (loadedEnemies.Count == 0)
        {
            Debug.LogWarning("No enemies loaded from save data.");
        }
        else
        {
            Debug.Log($"Loaded {loadedEnemies.Count} enemies from save data.");
        }
        // load the party from the save data
        List<PlayerController> loadedParty = new();

        foreach (var savedChar in saveData.Party)
        {
            GameObject go = Instantiate(_gameManager.PlayerPrefab, savedChar.Position, Quaternion.identity, _gameManager.PlayerContainer.transform);
            PlayerController player = go.GetComponent<PlayerController>();

            player.Initialize(savedChar);

            loadedParty.Add(player);
        }
        if (loadedParty.Count == 0)
        {
            Debug.LogWarning("No characters loaded from save data.");
        }
        else
        {
            Debug.Log($"Loaded {loadedParty.Count} characters from save data.");
        }

        _gameManager.LoadGame(saveData);
    }

    private string GetSaveFilePath(string saveID)
    {
        return Path.Combine(GameSavePath, saveID + "_save.json");
    }

    private string CreateUniqueSaveID()
    {
        DateTime now = DateTime.Now;
        var id = $"{now:yyyyMMdd_HHmmss}_{System.Guid.NewGuid()}";
        return GetSaveFilePath(id);
    }
}

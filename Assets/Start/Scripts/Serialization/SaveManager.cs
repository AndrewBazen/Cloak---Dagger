
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
    public CharacterDatabase characterDatabase;
    public SaveDatabase saveDatabase;
    private List<SaveData> _saves;
    private GameManager _gameManager;

    private string GameSavePath => Application.persistentDataPath + "/GameSaves";

    public List<SaveData> Saves
    {
        get => _saves;
        set
        {
            _saves = value;
            SaveRegistry.OnSavesUpdated?.Invoke(_saves);
        }
    }
    private void Awake()
    {
        _gameManager = GameManager.Instance;
        Initialize();
    }

    private void Initialize()
    {
        // Ensure the save directory exists
        if (!Directory.Exists(Application.persistentDataPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath);
        }
        SaveRegistry.OnSaveRegistered += HandleRegister;
        SaveRegistry.OnSaveUnregistered += HandleUnregister;
    }

    public void SaveGame(string saveName)
    {
        SaveData saveData = new SaveData();

        foreach (var player in _gameManager.Party.Party)
        {
            saveData.Party.Add(new CharacterSaveData
            {
                characterId = player.characterData.Id,
                currentHealth = player.CurrentHealth,
                position = new float[] {
                    player.transform.position.x,
                    player.transform.position.y,
                    player.transform.position.z
                }
            });
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
        _gameManager.CurrentLevelData = saveData.CurrentLevel;
        if (_gameManager.CurrentLevelData == null)
        {
            Debug.LogError("Unable to load level. Data not found or corrupted.");
            return;
        }
        // load the enemeies
        List<EnemySaveData> loadedEnemies = new List<EnemySaveData>();
        foreach (var enemyData in saveData.Enemies)
        {
            GameObject enemy = Instantiate(_gameManager.EnemyPrefab, position: enemyData.position, parent: _gameManager.EnemyContainer.transform);
            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                enemy.transform.position = new Vector3(
                    enemyData.position[0],
                    enemyData.position[1],
                    enemyData.position[2]
                );
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
            CharacterData data = characterDatabase.GetById(savedChar.characterId);
            if (data == null)
            {
                Debug.LogError($"CharacterData not found for ID: {savedChar.characterId}");
                continue;
            }

            GameObject go = Instantiate(_gameManager.PlayerPrefab, position: savedChar.position, rotation: savedChar.rotation, parent: _gameManager.PlayerContainer.transform);
            PlayerController controller = go.GetComponent<PlayerController>();

            go.transform.position = new Vector3(
                savedChar.position[0],
                savedChar.position[1],
                savedChar.position[2]
            );

            loadedParty.Add(controller);
        }
        if (loadedParty.Count == 0)
        {
            Debug.LogWarning("No characters loaded from save data.");
        }
        else
        {
            Debug.Log($"Loaded {loadedParty.Count} characters from save data.");
        }

        _gameManager.LoadGame(loadedParty, loadedEnemies);
    }

    private void HandleRegister(string saveID)
    {
        Debug.Log($"Registered save ID: {saveID}");
        saveDatabase.RegisterSaveID(saveID);
    }

    private void HandleUnregister(string saveID)
    {
        Debug.Log($"Unregistered save ID: {saveID}");
        saveDatabase.UnregisterSaveID(saveID);
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

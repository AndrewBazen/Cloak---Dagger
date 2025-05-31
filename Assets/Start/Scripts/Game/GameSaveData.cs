// GameSaveData.cs
[System.Serializable]
public class GameSaveData
{
    public List<CharacterSaveData> party;
    public string currentScene;
    public int gold;
}

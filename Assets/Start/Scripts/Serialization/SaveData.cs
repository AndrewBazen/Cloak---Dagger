using System.Collections.Generic;
using Start.Scripts.Game;

public class SaveData
{
    public List<CharacterSaveData> Party;
    public List<LevelSaveData> CurrentLevel;
    public List<EnemySaveData> Enemies;
    public GameState GameState;
}
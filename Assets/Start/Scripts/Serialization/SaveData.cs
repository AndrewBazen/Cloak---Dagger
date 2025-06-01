using System.Collections.Generic;
using Start.Scripts.Game;
using Start.Scripts.Character;

public class SaveData
{
    public List<CharacterInfoData> Party;
    public List<LevelData> CurrentLevel;
    public List<EnemyData> Enemies;
    public GameState GameState;
}
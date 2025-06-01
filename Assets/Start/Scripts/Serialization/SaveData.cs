using System.Collections.Generic;
using Start.Scripts.Game;
using Start.Scripts.Character;
using Start.Scripts.Enemy;
using Start.Scripts.Levels;
using Start.Scripts.BaseClasses;

public class SaveData
{
    public List<CharacterInfoData> Party;
    public CharacterInfoData ActivePlayer;
    public List<Actor> TurnOrder;
    public LevelData CurrentLevel;
    public List<EnemyData> Enemies;
    public GameManager.GameState CurrentGameState;
}
using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts
{
    public class Strategy
    {
        public float StrategyAttackValue;
        public float StrategyMovementValue;
        public OverlayTile TargetTile;
        public OverlayTile CurrentTile;
        public GameObject PlayerToAttack;
        public Ability AbilityToUse;

        public float StrategyValue => StrategyAttackValue + StrategyMovementValue;

        public Strategy(OverlayTile targetTile, OverlayTile currentTile, GameObject playerToAttack,
            Ability abilityToUse, float strategyAttackValue = -100, float strategyMovementValue = -100)
        {
            PlayerToAttack = playerToAttack;
            StrategyAttackValue = strategyAttackValue;
            StrategyMovementValue = strategyMovementValue;
            TargetTile = targetTile;
            CurrentTile = currentTile;
            AbilityToUse = abilityToUse;
        }

        public Strategy()
        {
            StrategyAttackValue = -100000;
            StrategyMovementValue = -100000;
            TargetTile = null;
            CurrentTile = null;
            PlayerToAttack = null;
            AbilityToUse = null;
        }

        public override string ToString()
        {
            return $"StrategyValue {StrategyValue}\nTargetTile {TargetTile.Grid2DLocation}\nCurrentTile {CurrentTile.Grid2DLocation}";
        }
    }
}
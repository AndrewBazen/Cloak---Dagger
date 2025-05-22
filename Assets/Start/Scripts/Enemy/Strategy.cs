using UnityEngine;

namespace Start.Scripts.Enemy
{
    /// <summary>
    /// Represents an AI decision structure containing information about moves and attacks
    /// </summary>
    public class Strategy
    {
        // Target tile to move to
        public OverlayTile TargetTile { get; set; }
        
        // Current tile the entity is standing on
        public OverlayTile CurrentTile { get; set; }
        
        // Player to attack if any
        public GameObject PlayerToAttack { get; set; }
        
        // Ability to use if any
        public Ability AbilityToUse { get; set; }
        
        // Value of attacking - higher is better
        public float StrategyAttackValue { get; set; }
        
        // Value of movement - lower is better
        public float StrategyMovementValue { get; set; }
        
        // Combined strategy value (calculated property)
        public float StrategyValue => StrategyAttackValue - StrategyMovementValue;

        // Default constructor
        public Strategy()
        {
            TargetTile = null;
            CurrentTile = null;
            PlayerToAttack = null;
            AbilityToUse = null;
            StrategyAttackValue = 0f;
            StrategyMovementValue = 0f;
        }

        // Full constructor
        public Strategy(OverlayTile targetTile, OverlayTile currentTile, GameObject playerToAttack, 
            Ability abilityToUse, float strategyAttackValue, float strategyMovementValue)
        {
            TargetTile = targetTile;
            CurrentTile = currentTile;
            PlayerToAttack = playerToAttack;
            AbilityToUse = abilityToUse;
            StrategyAttackValue = strategyAttackValue;
            StrategyMovementValue = strategyMovementValue;
        }

        // Debug-friendly string representation
        public override string ToString()
        {
            return $"Strategy: Attack={StrategyAttackValue}, Movement={StrategyMovementValue}, " +
                   $"Total={StrategyValue}, HasTarget={PlayerToAttack != null}";
        }
    }
} 
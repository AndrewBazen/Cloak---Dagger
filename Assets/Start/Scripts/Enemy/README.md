# Enemy AI System

This directory contains the implementation of the enemy AI system for Cloak & Dagger. The system uses the Strategy pattern to define different AI behaviors for different types of enemies.

## Architecture

The AI system consists of the following components:

- **EnemyController**: The main component attached to enemy game objects, handling movement, attacks, and integration with the GameManager.
- **IAIStrategy**: Interface that all AI strategies implement.
- **BaseAIStrategy**: Abstract base class that provides common utilities and methods for concrete strategies.
- **Concrete Strategies**: Implementations like MeleeAIStrategy, RangedAIStrategy, etc. that define specific AI behaviors.
- **AIStrategyFactory**: Factory class that creates the appropriate strategy based on enemy type.
- **Strategy**: Data structure that represents the decision made by an AI strategy.

## How It Works

1. Each enemy has an EnemyController that is initialized with a reference to the GameManager.
2. During initialization, the EnemyController gets the appropriate AI strategy from the AIStrategyFactory based on its type.
3. When it's the enemy's turn, the EnemyController calls the strategy's EvaluateStrategy method.
4. The strategy evaluates the game state and returns a Strategy object containing the decision (move here, attack this player, etc.).
5. The EnemyController executes the strategy by moving to the target tile and/or attacking the target player.

## Adding New AI Strategies

To add a new AI strategy:

1. Create a new class that extends BaseAIStrategy and implements IAIStrategy.
2. Implement the EvaluateStrategy method to return a Strategy object.
3. Register the new strategy in AIStrategyFactory.GetStrategy().

## Example

```csharp
public class NewEnemyAIStrategy : BaseAIStrategy
{
    public override Strategy EvaluateStrategy(EnemyController enemy, GameManager gameManager)
    {
        // Evaluate the game state and decide what to do
        // ...
        
        // Return a strategy object
        return new Strategy(targetTile, currentTile, playerToAttack, abilityToUse, 
            attackValue, movementValue);
    }
}
```

Then add it to the factory:

```csharp
public static IAIStrategy GetStrategy(string enemyType)
{
    // ...
    case "newtype":
        return new NewEnemyAIStrategy();
    // ...
}
```

## Debugging

Each Strategy object has a ToString() method that provides useful debug information about the decision making. You can use Debug.Log(strategy) to see the details of a strategy in the Unity console. 
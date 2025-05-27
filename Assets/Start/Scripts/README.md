# Cloak & Dagger Code Organization

This document describes the folder structure and organization of the codebase for better maintainability.

## Folder Structure

The codebase is organized into the following logical folders:

### Character

Contains scripts related to player characters and NPCs:

- `CharacterInfo.cs` - Base class for character stats and attributes
- `PlayerController.cs` - Player movement and input handling

### Combat

Contains scripts related to the combat system:

- `CombatController.cs` - Handles attacks, damage, and combat mechanics
- `TurnOrder.cs` - Manages initiative and turn-based combat flow

### Dice

Contains scripts related to the dice rolling system:

- `DiceRoll.cs` - Core dice mechanics for the game
- `DiceController.cs` - UI and visual representation of dice

### Enemy

Contains scripts related to enemies:

- `EnemyController.cs` - AI and behavior for enemy characters
- `EnemyData.cs` - Data structures for enemy stats and attributes
- `SpawnEnemies.cs` - Handles enemy spawning in the game

### Game

Contains core game systems:

- `GameEvents.cs` - Event system for game-wide communication
- `GameManager.cs` - Main game state and management

### Inventory

Contains inventory-related scripts:

- `InventoryHolder.cs` - Manages character inventories
- `InventoryItemData.cs` - Data structure for items

### Item

Contains item-related scripts:

- `Item.cs` - Base class for game items
- `ItemFactory.cs` - Factory for creating items

### Map

Contains scripts related to the game map:

- `MapManager.cs` - Manages the game map and grid
- `OverlayTile.cs` - Visual representation and data for map tiles
- `PathFinder.cs` - Handles pathfinding for character movement
- `RangeFinder.cs` - Calculates movement and attack ranges

### UI

Contains UI-related scripts:

- `UIManager.cs` - Manages UI elements
- `StatDisplay.cs` - Displays character stats

## Coding Standards

To maintain code quality, please follow these guidelines:

1. Place new scripts in the appropriate folder based on their functionality
2. Use proper namespaces matching the folder structure
3. Keep classes focused on a single responsibility
4. Comment complex code sections
5. Use consistent naming conventions:
   - PascalCase for class names and public members
   - camelCase for private fields
   - _camelCase for private fields with underscore prefix

## Adding New Features

When adding new features:

1. Create files in the appropriate folder
2. Update namespaces to match the folder structure
3. Add references to the README if needed


# Unity Optimization Plan for Cloak & Dagger

This document outlines the optimization steps being implemented to update the project for the latest version of Unity.

## 1. Rendering Upgrades

### Universal Render Pipeline (URP)
- Install URP package
- Create URP asset and renderer
- Upgrade all materials for URP compatibility
- Configure lighting and shadows for 2D

## 2. Input System Modernization

### New Input System
- Install Input System package
- Create input action assets for player controls
- Update player controller to use the new input system
- Implement input rebinding support

## 3. AI System Optimizations

### JobSystem Integration
- Implement IJob interfaces for heavy AI calculations
- Refactor pathfinding to use Burst-compilable code
- Create object pooling for AI strategies

### NavMesh for 2D
- Install NavMeshComponents
- Configure 2D NavMesh surfaces
- Update enemies to use NavMesh for pathfinding

## 4. Memory Management

### Object Pooling
- Create pooling systems for frequently instantiated objects:
  - Enemies
  - Projectiles
  - VFX
  - UI elements
- Implement IObjectPool interface

## 5. Code Modernization

### Replace Legacy Patterns
- Replace FindGameObjectWithTag with cached references
- Use TryGetComponent instead of GetComponent
- Implement async/await pattern for asynchronous operations
- Use C# 9.0+ features (records, pattern matching)

## 6. Project Settings Updates

### .NET Configuration
- Update to .NET Standard 2.1
- Enable Assembly Version Validation
- Configure incremental garbage collection
- Enable Assembly Updater

### Performance Settings
- Enable Burst Compilation
- Configure appropriate quality settings
- Set up asset loading optimizations

## 7. Build Pipeline Improvements

### Addressables
- Install Addressables package
- Configure content groups
- Implement asset loading through Addressables

## Implementation Progress

- [x] Universal Render Pipeline - Added package and created assets
- [x] New Input System - Added package and created input actions
- [x] AI JobSystem Integration - Created PathfindingJob and JobSystemPathFinder
- [ ] NavMesh for 2D
- [x] Object Pooling - Created ObjectPool, PoolableObject, and PoolManager
- [ ] Code Modernization
- [x] Project Settings Updates - Created guidance for .NET settings
- [x] Addressables System - Added package

## Next Steps

1. Update enemy and player scripts to use the new JobSystemPathFinder instead of the original PathFinder.
2. Modernize GameObject references using ObjectPooling for enemies and projectiles.
3. Apply material upgrades for URP compatibility.
4. Update controllers to use the new Input System.

## Known Issues

1. When upgrading to URP, some sprites may need their materials updated. Use the "Edit > Rendering > Materials > Convert All Built-in Materials to URP" menu option.
2. The Jobs-based pathfinding requires testing with the actual MapManager implementation.
3. The Input System requires PlayerInput components to be added to player objects. 
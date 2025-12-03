# Build Orchestrator - Design Plan

## Executive Summary

This document outlines the design for a **Build Orchestrator** that tracks status and decides the next action for each character. The orchestrator will coordinate feature execution across multiple characters, manage dependencies, prioritize tasks, and provide comprehensive status reporting.

## Goals

1. **Status Tracking**: Track execution status for all characters and features
2. **Decision Making**: Intelligently decide what action to take next for each character
3. **Dependency Management**: Handle feature dependencies and execution order
4. **Concurrent Execution**: Manage multiple characters running simultaneously
5. **Reporting**: Provide real-time status updates and completion metrics

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Orchestrator Service                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Decision   │  │   Status     │  │   Execution  │      │
│  │   Engine     │  │   Tracker    │  │   Manager    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         └────────────────────┼────────────────────┘
                              │
         ┌────────────────────┼────────────────────┐
         │                    │                    │
    ┌────▼────┐         ┌─────▼─────┐        ┌─────▼─────┐
    │Character│         │Character  │        │Character  │
    │   1     │         │    2      │        │    N      │
    └─────────┘         └───────────┘        └───────────┘
         │                    │                    │
         └────────────────────┼────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │ AutomationService │
                    └───────────────────┘
```

## Core Components

### 1. Orchestrator Service (`IOrchestratorService` / `OrchestratorService`)

**Responsibilities:**
- Coordinate execution across all characters
- Manage execution queue and priorities
- Handle start/stop operations
- Provide status reporting

**Key Methods:**
```csharp
public interface IOrchestratorService
{
    // Start orchestrator for all active characters
    Task StartOrchestrationAsync(CancellationToken ct = default);
    
    // Stop orchestrator and all running characters
    Task StopOrchestrationAsync();
    
    // Get next action for a character
    Task<NextAction> GetNextActionAsync(string characterId);
    
    // Get overall status
    Task<OrchestrationStatus> GetStatusAsync();
    
    // Get character-specific status
    Task<CharacterOrchestrationStatus> GetCharacterStatusAsync(string characterId);
    
    // Register character for orchestration
    void RegisterCharacter(string characterId);
    
    // Unregister character
    void UnregisterCharacter(string characterId);
}
```

### 2. Decision Engine (`IDecisionEngine` / `DecisionEngine`)

**Responsibilities:**
- Analyze character state and determine next action
- Evaluate feature dependencies
- Calculate execution priority
- Handle edge cases (failures, retries, etc.)

**Decision Logic:**
1. Check if character is running
2. Check daily reset (new day = reset all statuses)
3. Get enabled features
4. Filter out completed features
5. Apply dependency rules
6. Calculate priority scores
7. Return highest priority action

**Key Methods:**
```csharp
public interface IDecisionEngine
{
    // Determine next action for character
    NextAction DetermineNextAction(CharacterAggregate character);
    
    // Check if feature can execute (dependencies met)
    bool CanExecuteFeature(CharacterAggregate character, FeatureType feature);
    
    // Get execution priority for feature
    int GetPriority(FeatureType feature);
    
    // Get feature dependencies
    IEnumerable<FeatureType> GetDependencies(FeatureType feature);
}
```

### 3. Status Tracker (`IStatusTracker` / `StatusTracker`)

**Responsibilities:**
- Track execution status for all characters
- Maintain execution history
- Calculate completion metrics
- Provide status snapshots

**Key Methods:**
```csharp
public interface IStatusTracker
{
    // Update character status
    void UpdateCharacterStatus(string characterId, CharacterExecutionStatus status);
    
    // Get current status for character
    CharacterExecutionStatus GetCharacterStatus(string characterId);
    
    // Get overall statistics
    OrchestrationStatistics GetStatistics();
    
    // Get completion percentage
    double GetCompletionPercentage(string characterId);
}
```

### 4. Execution Manager (`IExecutionManager` / `ExecutionManager`)

**Responsibilities:**
- Execute actions for characters
- Manage concurrent execution limits
- Handle retries and failures
- Coordinate with AutomationService

**Key Methods:**
```csharp
public interface IExecutionManager
{
    // Execute next action for character
    Task<ExecutionResult> ExecuteActionAsync(string characterId, NextAction action, CancellationToken ct);
    
    // Check if character can execute (not already running)
    bool CanExecute(string characterId);
    
    // Get active execution count
    int GetActiveExecutionCount();
}
```

## Feature Dependencies and Priority

### Dependency Rules

Features may depend on other features completing first. Example dependencies:

```
VipPromotion (Priority 1) - No dependencies
    ↓
AutoPhuBan (Priority 2) - May depend on VipPromotion
    ↓
TuHanh (Priority 3) - May depend on AutoPhuBan
    ↓
DoiNangNo (Priority 4) - May depend on TuHanh
    ↓
TrongNL (Priority 5) - May depend on DoiNangNo
    ↓
CheMatBao (Priority 6) - May depend on TrongNL
```

### Priority Levels

1. **Critical (Priority 1-3)**: Must-do daily tasks
   - VipPromotion (Priority 1)
   - AutoPhuBan (Priority 2)
   - TuHanh (Priority 3)

2. **Important (Priority 4-6)**: High-value features
   - DoiNangNo (Priority 4)
   - TrongNL (Priority 5)
   - CheMatBao (Priority 6)

3. **Standard (Priority 7-10)**: Regular features
   - TruMa (Priority 7)
   - RutBo (Priority 8)

4. **Optional (Priority 11+)**: Nice-to-have features
   - CauCa (Priority 11)
   - HaiThuoc (Priority 12)
   - MeTran (Priority 13)
   - TriAn (Priority 14)

### Default Execution Order

Based on priority system and typical game flow:

**Critical Priority (1-3):**
1. VipPromotion (Priority 1) - Receive VIP rewards first
2. AutoPhuBan (Priority 2) - Dungeon runs
3. TuHanh (Priority 3) - Cultivation

**Important Priority (4-6):**
4. DoiNangNo (Priority 4) - Exchange energy
5. TrongNL (Priority 5) - Planting materials
6. CheMatBao (Priority 6) - Crafting

**Standard Priority (7-10):**
7. TruMa (Priority 7) - Demon slaying quest
8. RutBo (Priority 8) - Equipment withdrawal
9. DoiKGDK (Priority 9) - Space-time exchange
10. LatTheBai (Priority 10) - Card flipping

**Optional Priority (11+):**
11. CauCa (Priority 11) - Fishing
12. HaiThuoc (Priority 12) - Herb gathering
13. MeTran (Priority 13) - Maze
14. TriAn (Priority 14) - Thanksgiving quest
15. AoMaThap (Priority 15) - Demon tower
16. TrongCay (Priority 16) - Tree planting
17. UocNguyen (Priority 17) - Wish
18. NhanThuongHLVT (Priority 18) - Guild rewards
19. NhanHoiPhuc (Priority 19) - Recovery
20. AutoThanTu (Priority 20) - Divine cultivation
21. DauPet (Priority 21) - Pet battle
22. BugOnline (Priority 22) - Online bug

## Data Models

### NextAction

```csharp
public class NextAction
{
    public string CharacterId { get; set; }
    public FeatureType? Feature { get; set; }  // null = no action needed
    public ActionType ActionType { get; set; } // Execute, Wait, Skip, Stop
    public int Priority { get; set; }
    public string Reason { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum ActionType
{
    ExecuteFeature,    // Execute a feature
    Wait,              // Wait (dependencies not met, cooldown, etc.)
    Skip,              // Skip (not enabled, already completed)
    Stop,              // Stop (error, user request)
    Complete           // All features completed
}
```

### OrchestrationStatus

```csharp
public class OrchestrationStatus
{
    public bool IsRunning { get; set; }
    public int TotalCharacters { get; set; }
    public int ActiveCharacters { get; set; }
    public int CompletedCharacters { get; set; }
    public int FailedCharacters { get; set; }
    public Dictionary<string, CharacterOrchestrationStatus> CharacterStatuses { get; set; }
    public OrchestrationStatistics Statistics { get; set; }
    public DateTime LastUpdate { get; set; }
}
```

### CharacterOrchestrationStatus

```csharp
public class CharacterOrchestrationStatus
{
    public string CharacterId { get; set; }
    public bool IsActive { get; set; }
    public FeatureType? CurrentFeature { get; set; }
    public FeatureExecutionStatus? CurrentFeatureStatus { get; set; }
    public int CompletedFeatures { get; set; }
    public int TotalEnabledFeatures { get; set; }
    public double CompletionPercentage { get; set; }
    public NextAction NextAction { get; set; }
    public DateTime LastUpdate { get; set; }
    public List<FeatureExecutionLog> ExecutionLog { get; set; }
}
```

### OrchestrationStatistics

```csharp
public class OrchestrationStatistics
{
    public int TotalFeaturesEnabled { get; set; }
    public int TotalFeaturesCompleted { get; set; }
    public int TotalFeaturesFailed { get; set; }
    public double OverallCompletionPercentage { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public Dictionary<FeatureType, FeatureStatistics> FeatureStats { get; set; }
}
```

## Implementation Plan

### Phase 1: Core Infrastructure

1. **Create interfaces** (`IOrchestratorService`, `IDecisionEngine`, `IStatusTracker`, `IExecutionManager`)
2. **Implement DecisionEngine** with basic dependency rules
3. **Implement StatusTracker** for status tracking
4. **Create data models** (NextAction, OrchestrationStatus, etc.)

### Phase 2: Orchestrator Service

1. **Implement OrchestratorService** with basic coordination
2. **Integrate with AutomationService**
3. **Add character registration/unregistration**
4. **Implement start/stop functionality**

### Phase 3: Execution Management

1. **Implement ExecutionManager** for concurrent execution
2. **Add retry logic** for failed features
3. **Add execution limits** (max concurrent characters)
4. **Handle cancellation** and cleanup

### Phase 4: Advanced Features

1. **Add dependency resolution** (complex dependency graphs)
2. **Add priority scoring** (dynamic priorities based on time, resources, etc.)
3. **Add execution history** and analytics
4. **Add status reporting UI** integration

### Phase 5: Integration and Testing

1. **Integrate with Form1** (UI integration)
2. **Add status display** in main form
3. **Add logging** and error handling
4. **Comprehensive testing** with multiple characters

## File Structure

```
v1/Services/Orchestrator/
├── IOrchestratorService.cs          # Main orchestrator interface
├── OrchestratorService.cs           # Main orchestrator implementation
├── IDecisionEngine.cs               # Decision engine interface
├── DecisionEngine.cs                # Decision engine implementation
├── IStatusTracker.cs                # Status tracker interface
├── StatusTracker.cs                 # Status tracker implementation
├── IExecutionManager.cs             # Execution manager interface
├── ExecutionManager.cs              # Execution manager implementation
├── Models/
│   ├── NextAction.cs                # Next action data model
│   ├── OrchestrationStatus.cs       # Overall status model
│   ├── CharacterOrchestrationStatus.cs  # Character status model
│   ├── OrchestrationStatistics.cs   # Statistics model
│   └── FeatureStatistics.cs         # Feature-level statistics
├── DependencyRules.cs               # Feature dependency definitions
└── PriorityRules.cs                 # Feature priority definitions
```

## Usage Example

```csharp
// Initialize orchestrator
var orchestrator = new OrchestratorService(
    automationService,
    characterRepository,
    windowManager,
    logger
);

// Register characters
foreach (var character in characters)
{
    orchestrator.RegisterCharacter(character.Id);
}

// Start orchestration
await orchestrator.StartOrchestrationAsync(cancellationToken);

// Get status
var status = await orchestrator.GetStatusAsync();
Console.WriteLine($"Completion: {status.Statistics.OverallCompletionPercentage}%");

// Get next action for character
var nextAction = await orchestrator.GetNextActionAsync("Character1");
if (nextAction.ActionType == ActionType.ExecuteFeature)
{
    Console.WriteLine($"Next: {nextAction.Feature}");
}

// Stop orchestration
await orchestrator.StopOrchestrationAsync();
```

## Benefits

1. **Centralized Control**: Single point of control for all character automation
2. **Intelligent Scheduling**: Automatic prioritization and dependency resolution
3. **Status Visibility**: Real-time status tracking and reporting
4. **Scalability**: Handle many characters efficiently
5. **Maintainability**: Clear separation of concerns
6. **Extensibility**: Easy to add new features and rules

## Next Steps

1. Review and approve this design
2. Implement Phase 1 (Core Infrastructure)
3. Test with single character
4. Expand to multiple characters
5. Add UI integration
6. Production deployment


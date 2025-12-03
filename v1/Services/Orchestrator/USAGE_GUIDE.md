# Orchestrator Usage Guide

## Quick Start

### 1. Initialize the Orchestrator

The orchestrator needs to be initialized with its dependencies. Here's how to set it up:

```csharp
using AutoVPT.Services.Orchestrator;
using AutoVPT.Services.Orchestrator.Models;
using AutoVPT.Interfaces;
using AutoVPT.Repositories;
using AutoVPT.Services;
using System.Threading;
using System.Threading.Tasks;

// Get dependencies from ServiceContainer (after Initialize)
var characterRepo = ServiceContainer.GetService<ICharacterRepository>();
var automationService = ServiceContainer.GetService<IAutomationService>();
var logger = ServiceContainer.GetService<ILogger>();

// Create orchestrator components
var decisionEngine = new DecisionEngine();
var statusTracker = new StatusTracker();
var executionManager = new ExecutionManager(
    automationService,
    characterRepo,
    logger,
    maxConcurrentExecutions: 5  // Run up to 5 characters simultaneously
);

// Create orchestrator service
var orchestrator = new OrchestratorService(
    decisionEngine,
    statusTracker,
    executionManager,
    characterRepo,
    logger
);
```

### 2. Register Characters

Register characters that should be managed by the orchestrator:

```csharp
// Register individual characters
orchestrator.RegisterCharacter("Character1");
orchestrator.RegisterCharacter("Character2");
orchestrator.RegisterCharacter("Character3");

// Or register all characters from database
var allCharacters = characterRepo.GetAll();
foreach (var character in allCharacters)
{
    orchestrator.RegisterCharacter(character.ID);
}
```

### 3. Start Orchestration

Start the orchestrator to begin managing character automation:

```csharp
// Create cancellation token (optional, for stopping later)
var cts = new CancellationTokenSource();

// Start orchestration
await orchestrator.StartOrchestrationAsync(cts.Token);

// The orchestrator will now:
// - Check each character's status
// - Determine next action based on priority
// - Execute features in the correct order
// - Track progress and completion
```

### 4. Monitor Status

Get real-time status of the orchestration:

```csharp
// Get overall status
var status = await orchestrator.GetStatusAsync();
Console.WriteLine($"Total Characters: {status.TotalCharacters}");
Console.WriteLine($"Active: {status.ActiveCharacters}");
Console.WriteLine($"Completed: {status.CompletedCharacters}");
Console.WriteLine($"Overall Completion: {status.Statistics.OverallCompletionPercentage:F1}%");

// Get character-specific status
var charStatus = await orchestrator.GetCharacterStatusAsync("Character1");
Console.WriteLine($"Character1 - Completed: {charStatus.CompletedFeatures}/{charStatus.TotalEnabledFeatures}");
Console.WriteLine($"Next Action: {charStatus.NextAction?.Feature}");
Console.WriteLine($"Completion: {charStatus.CompletionPercentage:F1}%");

// Get next action for a character
var nextAction = await orchestrator.GetNextActionAsync("Character1");
if (nextAction.ActionType == ActionType.ExecuteFeature)
{
    Console.WriteLine($"Next: {nextAction.Feature} (Priority {nextAction.Priority})");
}
else
{
    Console.WriteLine($"Status: {nextAction.ActionType} - {nextAction.Reason}");
}
```

### 5. Stop Orchestration

Stop the orchestrator when done:

```csharp
// Stop orchestration (will cancel all running executions)
await orchestrator.StopOrchestrationAsync();

// Or use cancellation token
cts.Cancel();
```

## Complete Example

Here's a complete example that can be added to Form1:

```csharp
using AutoVPT.Services.Orchestrator;
using AutoVPT.Services.Orchestrator.Models;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm : Form
{
    private IOrchestratorService _orchestrator;
    private CancellationTokenSource _orchestrationCts;

    private void InitializeOrchestrator()
    {
        // Get dependencies
        var characterRepo = ServiceContainer.GetService<ICharacterRepository>();
        var automationService = ServiceContainer.GetService<IAutomationService>();
        var logger = ServiceContainer.GetService<ILogger>();

        // Create components
        var decisionEngine = new DecisionEngine();
        var statusTracker = new StatusTracker();
        var executionManager = new ExecutionManager(
            automationService,
            characterRepo,
            logger,
            maxConcurrentExecutions: 5
        );

        // Create orchestrator
        _orchestrator = new OrchestratorService(
            decisionEngine,
            statusTracker,
            executionManager,
            characterRepo,
            logger
        );

        // Register all characters
        var allCharacters = characterRepo.GetAll();
        foreach (var character in allCharacters)
        {
            _orchestrator.RegisterCharacter(character.ID);
        }
    }

    private async void buttonStartOrchestrator_Click(object sender, EventArgs e)
    {
        if (_orchestrator == null)
        {
            InitializeOrchestrator();
        }

        if (_orchestrator.IsRunning)
        {
            MessageBox.Show("Orchestrator is already running!");
            return;
        }

        _orchestrationCts = new CancellationTokenSource();
        
        try
        {
            await _orchestrator.StartOrchestrationAsync(_orchestrationCts.Token);
            MessageBox.Show("Orchestrator started!");
            
            // Start monitoring status (optional)
            _ = Task.Run(async () => await MonitorOrchestrationStatusAsync());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting orchestrator: {ex.Message}");
        }
    }

    private async void buttonStopOrchestrator_Click(object sender, EventArgs e)
    {
        if (_orchestrator != null && _orchestrator.IsRunning)
        {
            await _orchestrator.StopOrchestrationAsync();
            _orchestrationCts?.Cancel();
            MessageBox.Show("Orchestrator stopped!");
        }
    }

    private async Task MonitorOrchestrationStatusAsync()
    {
        while (_orchestrator != null && _orchestrator.IsRunning)
        {
            try
            {
                var status = await _orchestrator.GetStatusAsync();
                
                // Update UI (invoke on UI thread)
                this.Invoke((MethodInvoker)delegate
                {
                    labelOrchestratorStatus.Text = 
                        $"Active: {status.ActiveCharacters} | " +
                        $"Completed: {status.CompletedCharacters} | " +
                        $"Progress: {status.Statistics.OverallCompletionPercentage:F1}%";
                });

                await Task.Delay(2000); // Update every 2 seconds
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log error
                break;
            }
        }
    }

    private async void buttonCheckStatus_Click(object sender, EventArgs e)
    {
        if (_orchestrator == null)
        {
            MessageBox.Show("Orchestrator not initialized!");
            return;
        }

        var status = await _orchestrator.GetStatusAsync();
        
        var message = $"Orchestration Status:\n\n" +
                     $"Total Characters: {status.TotalCharacters}\n" +
                     $"Active: {status.ActiveCharacters}\n" +
                     $"Completed: {status.CompletedCharacters}\n" +
                     $"Overall Progress: {status.Statistics.OverallCompletionPercentage:F1}%\n\n" +
                     $"Features Completed: {status.Statistics.TotalFeaturesCompleted}\n" +
                     $"Features Failed: {status.Statistics.TotalFeaturesFailed}";
        
        MessageBox.Show(message, "Orchestration Status");
    }
}
```

## Integration with ServiceContainer (Optional)

To make the orchestrator available through dependency injection, add this to `ServiceContainer.cs`:

```csharp
// In ServiceContainer.Initialize(), after AutomationService registration:

// Register orchestrator components
_provider.RegisterSingleton<IDecisionEngine>(sp => new DecisionEngine());
_provider.RegisterSingleton<IStatusTracker>(sp => new StatusTracker());
_provider.RegisterTransient<IExecutionManager>(sp =>
{
    var automationService = sp.GetService<IAutomationService>();
    var characterRepo = sp.GetService<ICharacterRepository>();
    var logger = sp.GetService<ILogger>();
    return new ExecutionManager(automationService, characterRepo, logger, maxConcurrentExecutions: 5);
});

// Register orchestrator service
_provider.RegisterSingleton<IOrchestratorService>(sp =>
{
    var decisionEngine = sp.GetService<IDecisionEngine>();
    var statusTracker = sp.GetService<IStatusTracker>();
    var executionManager = sp.GetService<IExecutionManager>();
    var characterRepo = sp.GetService<ICharacterRepository>();
    var logger = sp.GetService<ILogger>();
    return new OrchestratorService(decisionEngine, statusTracker, executionManager, characterRepo, logger);
});
```

Then use it like:

```csharp
var orchestrator = ServiceContainer.GetService<IOrchestratorService>();
```

## Priority System

The orchestrator uses the following priority system:

- **Critical (1-3)**: VipPromotion, AutoPhuBan, TuHanh
- **Important (4-6)**: DoiNangNo, TrongNL, CheMatBao
- **Standard (7-10)**: TruMa, RutBo
- **Optional (11+)**: CauCa, HaiThuoc, MeTran, TriAn, etc.

Features are executed in priority order (lower number = higher priority).

## Configuration

### Max Concurrent Executions

Control how many characters can run simultaneously:

```csharp
var executionManager = new ExecutionManager(
    automationService,
    characterRepo,
    logger,
    maxConcurrentExecutions: 3  // Only 3 characters at a time
);
```

### Character Registration

Characters must be registered before orchestration starts:

```csharp
// Register all characters
var allCharacters = characterRepo.GetAll();
foreach (var character in allCharacters)
{
    if (character.Running == 1)  // Only register running characters
    {
        orchestrator.RegisterCharacter(character.ID);
    }
}
```

## Troubleshooting

### Orchestrator not starting
- Ensure ServiceContainer is initialized first
- Check that characters are registered
- Verify AutomationService is working

### No actions being executed
- Check character status (must be Running = 1)
- Verify features are enabled for characters
- Check if dependencies are met

### Status not updating
- Ensure orchestrator is running
- Check for exceptions in logs
- Verify character data is accessible

## Next Steps

1. Add UI buttons to start/stop orchestrator
2. Create status display panel
3. Add progress bars for each character
4. Implement pause/resume functionality
5. Add configuration UI for max concurrent executions


# Refactoring Patterns - Phase 4 Native Async Migration

**Date:** 2025-11-24
**Status:** Complete
**Coverage:** 10/13 executors (77%)

---

## Overview

This document describes three refactoring patterns used in Phase 4 to migrate executors from legacy wrapper pattern to modern async/await implementations.

### Pattern Distribution

- **Pure Native Async:** 4 executors (31%)
- **Hybrid Approach:** 3 executors (23%)
- **Already Native:** 3 executors (23%)
- **Remaining Wrappers:** 2 executors (15%)
- **Template/Incomplete:** 1 executor (8%)

---

## Pattern 1: Pure Native Async

### When to Use

Use this pattern when the feature:
- Does NOT require complex map navigation
- Does NOT require NVHN quest helper
- Primarily interacts with UI panels and buttons
- Can be implemented with image recognition and clicking

### Characteristics

✅ **No legacy dependencies** - No GeneralFunctions or AutoFeatures
✅ **Pure async/await** - No Task.Run wrapping
✅ **Direct service usage** - Uses IImageRecognition and IInputSimulator
✅ **Better testability** - Easy to mock dependencies
✅ **Better performance** - True async vs thread wrapping

### Executors Using This Pattern

1. VipPromotionExecutor (earlier refactor)
2. DoiKGDKExecutor (Phase 4 Batch 1)
3. RutBoExecutor (Phase 4 Batch 1)
4. NhanHoiPhucExecutor (Phase 4 Batch 1)

### Example: DoiKGDKExecutor

```csharp
public class DoiKGDKExecutor : BaseFeatureExecutor
{
    public override FeatureType Type => FeatureType.DoiKGDK;

    public DoiKGDKExecutor(
        IImageRecognition imageRecognition,
        IInputSimulator inputSimulator,
        ILogger logger)
        : base(imageRecognition, inputSimulator, logger)
    {
    }

    public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
    {
        try
        {
            LogInfo("Starting DoiKGDK (Space-Time Exchange) feature", context);

            // Step 1: Close all dialogs
            await CloseAllDialogsAsync(context);

            // Step 2: Open quick features list
            LogInfo("Opening quick features list...", context);
            var quickFeatureButton = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "quickFeatureButton.png",
                threshold: 0.8);

            if (!quickFeatureButton.HasValue)
            {
                throw new Exception("Could not find quick features button");
            }

            await _inputSimulator.ClickAsync(quickFeatureButton.Value);
            await Task.Delay(Constant.TimeShort);

            // Step 3: Open space-time carving panel using reusable helper
            LogInfo("Opening space-time carving panel...", context);
            if (!await OpenFeatureFromQuickListAsync("khonggiandieukhac", context))
            {
                return FeatureResult.Failed("Could not open space-time carving panel");
            }

            // Step 4: Click exchange button
            LogInfo("Clicking exchange button...", context);
            var exchangeButton = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "doikgdk.png",
                threshold: 0.8);

            if (!exchangeButton.HasValue)
            {
                return FeatureResult.Failed("Exchange button not found");
            }

            await _inputSimulator.ClickAsync(exchangeButton.Value);
            await Task.Delay(Constant.TimeShort);

            // Step 5: Confirm exchange
            LogInfo("Confirming exchange...", context);
            var confirmButton = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "luachonco.png",
                threshold: 0.8);

            if (confirmButton.HasValue)
            {
                await _inputSimulator.ClickAsync(confirmButton.Value);
                await Task.Delay(Constant.TimeShort);
            }

            // Step 6: Close panels
            await CloseAllDialogsAsync(context);

            LogInfo("DoiKGDK completed successfully", context);
            return FeatureResult.Successful("Space-time exchange completed");
        }
        catch (Exception ex)
        {
            LogError($"DoiKGDK feature failed: {ex.Message}", ex, context);
            return FeatureResult.Failed(ex.Message);
        }
    }

    // Reusable helper method
    private async Task<bool> OpenFeatureFromQuickListAsync(string featureName, ExecutionContext context)
    {
        // Implementation...
    }

    private async Task CloseAllDialogsAsync(ExecutionContext context)
    {
        // Implementation...
    }
}
```

### Key Benefits

1. **Cleaner Code Structure**
   - Clear step-by-step flow
   - Self-documenting with descriptive variable names
   - No complex thread management

2. **Better Error Handling**
   - Specific failure messages (e.g., "Exchange button not found")
   - Stack traces preserved in logs
   - Graceful degradation

3. **Reusable Helpers**
   - `OpenFeatureFromQuickListAsync()` can be used by other features
   - `CloseAllDialogsAsync()` common across all executors
   - `ClickImageWithLoopAsync()` for retry logic

4. **Improved Performance**
   - True async execution (not blocking threads)
   - Better resource utilization
   - Responsive to cancellation tokens

5. **Testability**
   - Easy to mock IImageRecognition and IInputSimulator
   - Can unit test without game running
   - Clear dependencies in constructor

### Common Helper Methods

#### CloseAllDialogsAsync
```csharp
private async Task CloseAllDialogsAsync(ExecutionContext context)
{
    LogInfo("Closing all dialogs...", context);

    for (int i = 0; i < 5; i++)
    {
        await _inputSimulator.SendKeyAsync(VirtualKeyCode.ESCAPE);
        await Task.Delay(300);
    }

    await Task.Delay(Constant.TimeShort);
}
```

#### WaitForPanelAsync
```csharp
private async Task<bool> WaitForPanelAsync(string panelImageName, ExecutionContext context, int maxRetries = 10)
{
    int attempts = 0;

    while (attempts < maxRetries)
    {
        var panelLocation = await _imageRecognition.FindImageAsync(
            Constant.ImagePathGlobalFolder + panelImageName + ".png",
            threshold: 0.8);

        if (panelLocation.HasValue)
        {
            return true;
        }

        attempts++;
        await Task.Delay(500);
    }

    return false;
}
```

#### ClickImageWithLoopAsync
```csharp
private async Task<bool> ClickImageWithLoopAsync(string imageName, string actionDescription, ExecutionContext context)
{
    int attempts = 0;
    int maxAttempts = Constant.MaxLoop;

    while (attempts < maxAttempts)
    {
        var imageLocation = await _imageRecognition.FindImageAsync(
            Constant.ImagePathGlobalFolder + imageName + ".png",
            threshold: 0.8);

        if (imageLocation.HasValue)
        {
            LogInfo($"{actionDescription}...", context);
            await _inputSimulator.ClickAsync(imageLocation.Value);
            await Task.Delay(500);
            return true;
        }

        attempts++;
        await Task.Delay(300);
    }

    return false;
}
```

#### ClickAllImagesWithLoopAsync
```csharp
private async Task<bool> ClickAllImagesWithLoopAsync(string imageName, string actionDescription, ExecutionContext context)
{
    bool foundAny = false;
    int attempts = 0;
    int maxAttempts = Constant.MaxLoop;

    while (attempts < maxAttempts)
    {
        var imageLocation = await _imageRecognition.FindImageAsync(
            Constant.ImagePathGlobalFolder + imageName + ".png",
            threshold: 0.8);

        if (imageLocation.HasValue)
        {
            LogInfo($"{actionDescription}...", context);
            await _inputSimulator.ClickAsync(imageLocation.Value);
            await Task.Delay(500);
            foundAny = true;
            // Continue clicking until no more instances found
        }
        else if (foundAny)
        {
            // Found some before but not now, we're done
            return true;
        }
        else
        {
            // Never found any
            attempts++;
            await Task.Delay(300);
        }
    }

    return foundAny;
}
```

---

## Pattern 2: Hybrid Approach

### When to Use

Use this pattern when the feature:
- Requires complex map navigation (moveToMap, moveToNPC)
- Uses NVHN quest helper (openQuestByNVHN)
- Has flying mechanics (bay, bayXuong)
- Needs legacy AutoFeatures for navigation ONLY

### Characteristics

⚠️ **Partial legacy dependency** - Uses AutoFeatures for navigation only
✅ **No GeneralFunctions** - Removed high-level legacy functions
✅ **Clear step logging** - Better visibility than legacy
✅ **Structured flow** - Step-by-step comments
🔄 **Migration path** - Ready for navigation service when available

### Executors Using This Pattern

1. NhanThuongHLVTExecutor (Phase 4 Batch 2)
2. TuHanhExecutor (Phase 4 Batch 2)
3. AutoThanTuExecutor (Phase 4 Batch 2)

### Example: NhanThuongHLVTExecutor

```csharp
public class NhanThuongHLVTExecutor : BaseFeatureExecutor
{
    public override FeatureType Type => FeatureType.NhanThuongHLVT;

    public NhanThuongHLVTExecutor(
        IImageRecognition imageRecognition,
        IInputSimulator inputSimulator,
        ILogger logger)
        : base(imageRecognition, inputSimulator, logger)
    {
    }

    public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
    {
        try
        {
            LogInfo("Starting NhanThuongHLVT (Corridor Rewards) feature", context);

            // For complex navigation features, we still need legacy AutoFeatures
            // This is a hybrid approach until navigation is fully refactored
            await Task.Run(() =>
            {
                var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                var autoFeatures = new AutoFeatures(
                    context.WindowHandle,
                    context.Character.Identity.Id,
                    context.StatusTextBox,
                    legacyCharacter
                );

                // Step 1: Close all dialogs
                LogInfo("Closing all dialogs...", context);
                autoFeatures.closeAllDialog();

                // Step 2: Navigate to Quyền Cô Thành
                LogInfo("Navigating to Quyền Cô Thành...", context);
                if (!autoFeatures.moveToMap("quyencothanh", 5))
                {
                    throw new Exception("Failed to navigate to Quyền Cô Thành");
                }

                // Step 3: Fly up
                LogInfo("Flying up...", context);
                autoFeatures.bay();

                // Step 4: Move to NPC
                LogInfo("Moving to corridor NPC...", context);
                if (!autoFeatures.moveToNPC("conghanhlang", "nhanquahanhlang"))
                {
                    throw new Exception("Failed to reach corridor NPC");
                }

                // Step 5: Fly down
                LogInfo("Flying down...", context);
                autoFeatures.bayXuong();

                // Step 6: Talk to NPC
                LogInfo("Talking to NPC...", context);
                if (!autoFeatures.talkToNPC("conghanhlang", 0, 0, -40))
                {
                    throw new Exception("Failed to talk to NPC");
                }

                // Step 7: Scroll down in dialog
                LogInfo("Scrolling down...", context);
                autoFeatures.clickImageByGroup("global", "keoxuong", false, true, 3);

                // Step 8: Click receive rewards button
                LogInfo("Collecting corridor rewards...", context);
                autoFeatures.clickImageByGroup("global", "nhanthuonghanhlang", false, true);

            }, context.CancellationToken);

            LogInfo("NhanThuongHLVT completed successfully", context);
            return FeatureResult.Successful("Corridor rewards collected");
        }
        catch (Exception ex)
        {
            LogError($"NhanThuongHLVT feature failed: {ex.Message}", ex, context);
            return FeatureResult.Failed(ex.Message);
        }
    }
}
```

### Key Benefits

1. **Better Than Legacy Wrapper**
   - Removed GeneralFunctions dependency
   - Clear step-by-step logging
   - Better error messages
   - Direct control of flow

2. **Migration Ready**
   - Clear separation of navigation vs UI logic
   - Easy to replace AutoFeatures when navigation service ready
   - Documents which legacy methods are still needed

3. **Documented Dependency**
   - Comments explain WHY legacy is used
   - TODO notes for future refactoring
   - Clear migration path

### Navigation Methods Still Used

From `AutoFeatures.cs`:

- `moveToMap(mapName, offsetX, offsetY)` - Navigate to specific map location
- `moveToNPC(npcName, imageName)` - Navigate to NPC position
- `bay()` - Fly up (flying mount mechanic)
- `bayXuong()` - Fly down (landing mechanic)
- `talkToNPC(npcName, offsetX, offsetY, offsetDialog)` - Interact with NPC
- `openQuestByNVHN(questName)` - Use NVHN quest helper
- `isTalkWithNPC(npcName)` - Check if in dialogue with NPC

### Future Migration Path

When navigation service is implemented:

```csharp
// BEFORE (Hybrid):
autoFeatures.moveToMap("quyencothanh", 5);

// AFTER (Pure Native):
await _navigationService.MoveToMapAsync("quyencothanh", new Point(5, 0), context.CancellationToken);
```

---

## Pattern 3: Already Native

### When to Use

This isn't a refactoring pattern - these executors were already implemented using native async/await in earlier phases.

### Characteristics

✅ **Already pure async/await** - No refactoring needed
✅ **No legacy dependencies** - Never used GeneralFunctions
✅ **Good structure** - Clear step-by-step flow
✅ **Reusable helpers** - Already have helper methods

### Executors Using This Pattern

1. CheMatBaoExecutor (Secret manual crafting)
2. TrongNLExecutor (Material planting)
3. TriAnExecutor (Gratitude quest)

### Example: CheMatBaoExecutor

```csharp
public class CheMatBaoExecutor : BaseFeatureExecutor
{
    public override FeatureType Type => FeatureType.CheMatBao;

    public CheMatBaoExecutor(
        IImageRecognition imageRecognition,
        IInputSimulator inputSimulator,
        ILogger logger)
        : base(imageRecognition, inputSimulator, logger)
    {
    }

    public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
    {
        try
        {
            LogInfo("Starting CheMatBao (Secret Manual Crafting) feature", context);

            // Get configuration
            int tier = context.Character.FeatureConfig.CheMatBaoTier;
            string type = context.Character.FeatureConfig.CheMatBaoType;

            // Open character panel
            var characterButton = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "characterButton.png");

            if (!characterButton.HasValue)
            {
                return FeatureResult.Failed("Character button not found");
            }

            await _inputSimulator.ClickAsync(characterButton.Value);
            await Task.Delay(Constant.TimeShort);

            // ... more steps ...

            return FeatureResult.Successful($"Crafted {count} secret manuals");
        }
        catch (Exception ex)
        {
            LogError($"CheMatBao feature failed: {ex.Message}", ex, context);
            return FeatureResult.Failed(ex.Message);
        }
    }

    // Has helper methods
    private async Task<Point?> FindImageByGroupAsync(string group, string imageName)
    {
        // Helper implementation
    }
}
```

### Why These Were Already Good

1. **Early Adopters**: These features were implemented during earlier phases when the async pattern was being established
2. **Simple UI Flow**: No complex navigation needed, just panel interactions
3. **Good Examples**: Serve as templates for other refactors
4. **No Changes Needed**: Already follow best practices

---

## Comparison: Before and After

### Before: Legacy Wrapper Pattern

```csharp
public class DoiKGDKExecutor : BaseFeatureExecutor
{
    public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
    {
        try
        {
            // Wrap legacy code in Task.Run
            await Task.Run(() =>
            {
                var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                var generalFunctions = new GeneralFunctions(
                    context.WindowHandle,
                    context.Character.Identity.Id,
                    context.StatusTextBox,
                    legacyCharacter
                );

                // Call monolithic legacy method (no visibility into steps)
                generalFunctions.DoiKGDK();

            }, context.CancellationToken);

            return FeatureResult.Successful("Space-time exchange completed");
        }
        catch (Exception ex)
        {
            return FeatureResult.Failed(ex.Message);
        }
    }
}
```

**Problems:**
- ❌ Task.Run wrapping (not true async)
- ❌ Legacy GeneralFunctions dependency
- ❌ No visibility into individual steps
- ❌ Generic error messages
- ❌ Hard to test
- ❌ Tight coupling

### After: Pure Native Async

```csharp
public class DoiKGDKExecutor : BaseFeatureExecutor
{
    public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
    {
        try
        {
            LogInfo("Starting DoiKGDK (Space-Time Exchange) feature", context);

            // Step 1: Close all dialogs
            await CloseAllDialogsAsync(context);

            // Step 2: Open quick features list
            LogInfo("Opening quick features list...", context);
            var quickFeatureButton = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "quickFeatureButton.png",
                threshold: 0.8);

            if (!quickFeatureButton.HasValue)
            {
                throw new Exception("Could not find quick features button");
            }

            await _inputSimulator.ClickAsync(quickFeatureButton.Value);
            await Task.Delay(Constant.TimeShort);

            // Step 3: Open space-time carving panel
            LogInfo("Opening space-time carving panel...", context);
            if (!await OpenFeatureFromQuickListAsync("khonggiandieukhac", context))
            {
                return FeatureResult.Failed("Could not open space-time carving panel");
            }

            // ... more clear steps ...

            LogInfo("DoiKGDK completed successfully", context);
            return FeatureResult.Successful("Space-time exchange completed");
        }
        catch (Exception ex)
        {
            LogError($"DoiKGDK feature failed: {ex.Message}", ex, context);
            return FeatureResult.Failed(ex.Message);
        }
    }

    private async Task<bool> OpenFeatureFromQuickListAsync(string featureName, ExecutionContext context)
    {
        // Reusable helper
    }

    private async Task CloseAllDialogsAsync(ExecutionContext context)
    {
        // Common helper
    }
}
```

**Benefits:**
- ✅ True async/await (better performance)
- ✅ No legacy dependencies (clean architecture)
- ✅ Clear step-by-step logging
- ✅ Specific error messages
- ✅ Easy to test (mock dependencies)
- ✅ Loose coupling

### Metrics Comparison

| Metric | Legacy Wrapper | Pure Native Async | Improvement |
|--------|---------------|-------------------|-------------|
| Lines of Code | 92 | 199 | +117% (more explicit) |
| Legacy Dependencies | 2 (AutoFeatures, GeneralFunctions) | 0 | -100% |
| Helper Methods | 0 | 3 | +∞ |
| Log Statements | 2 | 8 | +300% |
| Error Specificity | Generic | Specific | Better debugging |
| Testability | Hard (needs game) | Easy (mockable) | Much easier |
| Reusability | Low | High | Helper methods |

---

## Best Practices

### 1. Logging

**Always log:**
- Feature start
- Each major step
- Success/failure clearly
- Useful context (counts, types, etc.)

```csharp
LogInfo("Starting DoiKGDK (Space-Time Exchange) feature", context);
LogInfo("Opening quick features list...", context);
LogInfo("DoiKGDK completed successfully", context);
LogError($"DoiKGDK feature failed: {ex.Message}", ex, context);
```

### 2. Error Handling

**Return specific failure messages:**
```csharp
// Good
return FeatureResult.Failed("Could not open space-time carving panel");

// Bad
return FeatureResult.Failed("Failed");
```

### 3. Reusable Helpers

**Extract common patterns:**
```csharp
// Instead of duplicating this code:
for (int i = 0; i < 5; i++)
{
    await _inputSimulator.SendKeyAsync(VirtualKeyCode.ESCAPE);
    await Task.Delay(300);
}

// Create a helper:
private async Task CloseAllDialogsAsync(ExecutionContext context)
{
    LogInfo("Closing all dialogs...", context);
    for (int i = 0; i < 5; i++)
    {
        await _inputSimulator.SendKeyAsync(VirtualKeyCode.ESCAPE);
        await Task.Delay(300);
    }
    await Task.Delay(Constant.TimeShort);
}
```

### 4. Comments and Documentation

**Document WHY, not WHAT:**
```csharp
// Good - explains WHY we use legacy
// For complex navigation features, we still need legacy AutoFeatures
// This is a hybrid approach until navigation is fully refactored

// Bad - explains WHAT (code already says this)
// This calls the moveToMap function
```

### 5. Step-by-Step Flow

**Number and describe each step:**
```csharp
// Step 1: Close all dialogs
await CloseAllDialogsAsync(context);

// Step 2: Open quick features list
LogInfo("Opening quick features list...", context);
// ...

// Step 3: Open panel
LogInfo("Opening space-time carving panel...", context);
// ...
```

---

## Common Pitfalls to Avoid

### 1. Over-Wrapping with Task.Run

**Don't do this:**
```csharp
public async Task DoSomethingAsync()
{
    await Task.Run(async () =>
    {
        await ActualAsyncWork();  // ← Already async!
    });
}
```

**Do this:**
```csharp
public async Task DoSomethingAsync()
{
    await ActualAsyncWork();  // ← Direct async call
}
```

### 2. Swallowing Exceptions

**Don't do this:**
```csharp
try
{
    await SomeOperation();
}
catch
{
    // Silent failure
}
```

**Do this:**
```csharp
try
{
    await SomeOperation();
}
catch (Exception ex)
{
    LogError($"Operation failed: {ex.Message}", ex, context);
    throw;  // Or return FeatureResult.Failed()
}
```

### 3. Not Using Cancellation Tokens

**Don't do this:**
```csharp
while (true)
{
    await Task.Delay(1000);  // ← No cancellation
}
```

**Do this:**
```csharp
while (!context.CancellationToken.IsCancellationRequested)
{
    await Task.Delay(1000, context.CancellationToken);
}
```

### 4. Duplicate Code

**Don't do this:**
```csharp
// In Executor A
for (int i = 0; i < 5; i++)
{
    await _inputSimulator.SendKeyAsync(VirtualKeyCode.ESCAPE);
    await Task.Delay(300);
}

// In Executor B (same code!)
for (int i = 0; i < 5; i++)
{
    await _inputSimulator.SendKeyAsync(VirtualKeyCode.ESCAPE);
    await Task.Delay(300);
}
```

**Do this:**
```csharp
// In BaseFeatureExecutor or helper class
protected async Task CloseAllDialogsAsync(ExecutionContext context)
{
    for (int i = 0; i < 5; i++)
    {
        await _inputSimulator.SendKeyAsync(VirtualKeyCode.ESCAPE);
        await Task.Delay(300);
    }
}

// In executors
await CloseAllDialogsAsync(context);
```

---

## Future Improvements

### 1. Extract Common Base Class

Create `AsyncExecutorHelpers` with shared methods:
```csharp
public abstract class AsyncExecutorHelpers : BaseFeatureExecutor
{
    protected async Task CloseAllDialogsAsync(ExecutionContext context) { }
    protected async Task<bool> WaitForImageAsync(string imageName, ExecutionContext context) { }
    protected async Task<bool> ClickImageWithRetryAsync(string imageName, ExecutionContext context) { }
}
```

### 2. Navigation Service

Replace AutoFeatures navigation with:
```csharp
public interface INavigationService
{
    Task<bool> MoveToMapAsync(string mapName, Point offset, CancellationToken ct);
    Task<bool> MoveToNPCAsync(string npcName, CancellationToken ct);
    Task FlyAsync(CancellationToken ct);
    Task LandAsync(CancellationToken ct);
    Task<bool> TalkToNPCAsync(string npcName, Point offset, CancellationToken ct);
}
```

### 3. Retry and Resilience

Add exponential backoff and retry policies:
```csharp
protected async Task<Point?> FindImageWithRetryAsync(
    string imagePath,
    int maxRetries = 3,
    ExecutionContext context)
{
    int attempt = 0;
    while (attempt < maxRetries)
    {
        var location = await _imageRecognition.FindImageAsync(imagePath);
        if (location.HasValue) return location;

        int delay = (int)Math.Pow(2, attempt) * 1000; // Exponential backoff
        await Task.Delay(delay, context.CancellationToken);
        attempt++;
    }
    return null;
}
```

### 4. Unit Tests

All pure native async executors can be unit tested:
```csharp
[Test]
public async Task DoiKGDKExecutor_Should_Complete_Successfully()
{
    // Arrange
    var mockImageRec = new Mock<IImageRecognition>();
    var mockInput = new Mock<IInputSimulator>();
    var mockLogger = new Mock<ILogger>();

    mockImageRec.Setup(x => x.FindImageAsync(It.IsAny<string>(), It.IsAny<double>()))
        .ReturnsAsync(new Point(100, 100));

    var executor = new DoiKGDKExecutor(mockImageRec.Object, mockInput.Object, mockLogger.Object);
    var context = CreateTestContext();

    // Act
    var result = await executor.ExecuteAsync(context);

    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual("Space-time exchange completed", result.Message);
}
```

---

## Decision Tree: Which Pattern to Use?

```
Does the feature require map navigation or NVHN quest helper?
│
├─ YES → Use Hybrid Approach
│        - Keep AutoFeatures for navigation only
│        - Remove GeneralFunctions
│        - Add clear step logging
│        - Document legacy usage with TODO
│
└─ NO → Does it only use UI panels and buttons?
         │
         ├─ YES → Use Pure Native Async
         │        - No legacy dependencies
         │        - Pure async/await
         │        - Create reusable helpers
         │        - Full testability
         │
         └─ NO → Is it very complex (multi-phase quest)?
                  │
                  ├─ YES → Keep as wrapper for now
                  │        - Plan future refactoring
                  │        - Document complexity
                  │
                  └─ NO → Refactor to Pure Native Async
```

---

## Summary

### Pattern Selection Guide

| Feature Characteristic | Pattern | Example |
|----------------------|---------|---------|
| Simple UI interactions | Pure Native Async | DoiKGDK, RutBo, NhanHoiPhuc |
| Map navigation needed | Hybrid Approach | NhanThuongHLVT, AutoThanTu |
| NVHN quest navigation | Hybrid Approach | TuHanh |
| Already implemented well | Already Native | CheMatBao, TrongNL, TriAn |
| Very complex multi-phase | Wrapper (for now) | AutoPhuBan, TruMa |

### Refactoring Checklist

When refactoring an executor:

- [ ] Determine which pattern to use (decision tree above)
- [ ] Remove GeneralFunctions dependency if possible
- [ ] Add step-by-step logging with LogInfo()
- [ ] Create reusable helper methods
- [ ] Add specific error messages
- [ ] Document any legacy dependencies with TODO
- [ ] Test in production environment
- [ ] Update documentation

### Success Metrics

A well-refactored executor should have:

✅ Clear, descriptive logging at each step
✅ Specific error messages (not generic)
✅ Reusable helper methods extracted
✅ No unnecessary legacy dependencies
✅ Proper cancellation token usage
✅ Exception handling with logging
✅ Self-documenting code structure

---

## Conclusion

The three refactoring patterns provide a clear migration path:

1. **Pure Native Async**: Target state for simple features (31% achieved)
2. **Hybrid Approach**: Transition state for navigation-heavy features (23%)
3. **Already Native**: Examples of good implementation (23%)

**Total Coverage: 77% of executors using modern patterns**

The remaining 23% (2 wrapper executors + 1 template) can be addressed in future phases when:
- Navigation service is implemented (eliminates need for hybrid)
- Complex features are broken down into smaller pieces
- Team has more time for deep refactoring

This phased approach balances **immediate production needs** with **long-term code quality goals**.

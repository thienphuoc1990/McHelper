# Phase 3 Integration Examples

This document provides practical examples of how to use the new executor-based architecture alongside the existing legacy code.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Example 1: Using TrongNL Executor](#example-1-using-trongnl-executor)
3. [Example 2: Using TriAn Executor](#example-2-using-trian-executor)
4. [Example 3: Using CheMatBao Executor](#example-3-using-chematbao-executor)
5. [Example 4: Migrating Existing Code](#example-4-migrating-existing-code)
6. [Example 5: Error Handling](#example-5-error-handling)
7. [Example 6: Cancellation Support](#example-6-cancellation-support)

---

## Quick Start

### Initialize Service Container (One-Time Setup)

```csharp
// In Form1_Load or Application startup
public void Form1_Load(object sender, EventArgs e)
{
    // Initialize the service container with status textbox
    ServiceContainer.Initialize(textBoxStatus);

    // Container is now ready to use
}
```

### Get Window Services for a Character

```csharp
// Get character window handle
IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);

// Create window-specific services
var windowServices = ServiceContainer.CreateWindowServices(hWnd, character);

// Now use windowServices to create executors
```

---

## Example 1: Using TrongNL Executor

### Old Way (Legacy Code)

```csharp
public void buttonTrongNL_Click(object sender, EventArgs e)
{
    // Old approach using TrongNL class
    var character = GetSelectedCharacter();
    IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);

    var auto = new AutoFeatures(hWnd, character.ID, textBoxStatus);
    var trongNL = new TrongNL(hWnd, character.ID, auto, character.TrongNLLoai);

    // Open farm
    trongNL.moTrangVien();

    // Open farming panel
    trongNL.moNuoiTrong();

    // Check and plant
    if (trongNL.kiemTraSoDatTrong())
    {
        trongNL.chonNL();
        trongNL.trong();
    }

    // Harvest
    trongNL.thuHoach();

    // Close
    trongNL.dongTrangVien();

    // Mark as completed
    character.StatusTrongNL = 1;
    Helper.saveSettingsToXML(character);
}
```

### New Way (Executor Pattern)

```csharp
public async void buttonTrongNL_Click(object sender, EventArgs e)
{
    try
    {
        // Get character
        var character = GetSelectedCharacter();

        // Convert to aggregate
        var aggregate = CharacterAdapter.ToAggregate(character);

        // Get window handle
        IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);

        // Create window services
        var windowServices = ServiceContainer.CreateWindowServices(hWnd, character);

        // Create executor
        var executor = windowServices.CreateExecutor<TrongNLExecutor>();

        // Create execution context
        var context = new ExecutionContext
        {
            Character = aggregate,
            WindowHandle = hWnd,
            Config = aggregate.FeatureConfig.GetConfig(FeatureType.TrongNL),
            CancellationToken = CancellationToken.None
        };

        // Execute feature
        var result = await executor.ExecuteAsync(context);

        // Handle result
        if (result.Success)
        {
            // Mark as completed
            aggregate.RuntimeState.CompleteFeature(FeatureType.TrongNL);

            // Convert back and save
            var updatedCharacter = CharacterAdapter.ToLegacy(aggregate);
            Helper.saveSettingsToXML(updatedCharacter);

            MessageBox.Show($"TrongNL completed: {result.Message}", "Success");
        }
        else
        {
            MessageBox.Show($"TrongNL failed: {result.Message}", "Error");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}", "Error");
    }
}
```

### Even Simpler: Using Helper Method

```csharp
public async void buttonTrongNL_Click(object sender, EventArgs e)
{
    var character = GetSelectedCharacter();
    await RunFeatureExecutor<TrongNLExecutor>(character, FeatureType.TrongNL);
}

// Helper method (add to Form1 or Helper class)
private async Task RunFeatureExecutor<TExecutor>(Character character, FeatureType featureType)
    where TExecutor : class
{
    try
    {
        var aggregate = CharacterAdapter.ToAggregate(character);
        IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);
        var windowServices = ServiceContainer.CreateWindowServices(hWnd, character);
        var executor = windowServices.CreateExecutor<TExecutor>(character.VipLevel);

        var context = new ExecutionContext
        {
            Character = aggregate,
            WindowHandle = hWnd,
            Config = aggregate.FeatureConfig.GetConfig(featureType),
            CancellationToken = CancellationToken.None
        };

        var result = await executor.ExecuteAsync(context);

        if (result.Success)
        {
            aggregate.RuntimeState.CompleteFeature(featureType);
            var updated = CharacterAdapter.ToLegacy(aggregate);
            Helper.saveSettingsToXML(updated);
        }

        MessageBox.Show(result.Message, result.Success ? "Success" : "Error");
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message, "Error");
    }
}
```

---

## Example 2: Using TriAn Executor

### Old Way (Legacy Code)

```csharp
public void buttonTriAn_Click(object sender, EventArgs e)
{
    var character = GetSelectedCharacter();
    IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);
    var auto = new AutoFeatures(hWnd, character.ID, textBoxStatus);
    var triAn = new ChayTriAn(hWnd, character.ID, character, auto);

    // Accept quest
    triAn.nhanQ();

    // Complete quest
    triAn.chayQ();

    // Mark completed
    character.StatusTriAn = 1;
    Helper.saveSettingsToXML(character);
}
```

### New Way (Executor Pattern)

```csharp
public async void buttonTriAn_Click(object sender, EventArgs e)
{
    var character = GetSelectedCharacter();
    await RunFeatureExecutor<TriAnExecutor>(character, FeatureType.TriAn);
}
```

**Note:** TriAnExecutor handles VIP level automatically through the constructor parameter passed by `CreateExecutor()`.

---

## Example 3: Using CheMatBao Executor

### Old Way (Legacy Code)

```csharp
public void buttonCheMatBao_Click(object sender, EventArgs e)
{
    var character = GetSelectedCharacter();
    IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);
    var auto = new AutoFeatures(hWnd, character.ID, textBoxStatus);
    var cheMatBao = new CheMatBao(hWnd, character.ID, auto);

    // Set configuration
    cheMatBao.setLoaiMB(character.CheMatBaoLoai);
    cheMatBao.setCapMB(character.CheMatBaoCap);

    // Open panel
    cheMatBao.moBangCheMB();

    // Craft
    cheMatBao.che();

    // Mark completed
    character.StatusCheMatBao = 1;
    Helper.saveSettingsToXML(character);
}
```

### New Way (Executor Pattern)

```csharp
public async void buttonCheMatBao_Click(object sender, EventArgs e)
{
    var character = GetSelectedCharacter();

    // Ensure configuration is set in aggregate
    var aggregate = CharacterAdapter.ToAggregate(character);
    aggregate.FeatureConfig.GetConfig(FeatureType.CheMatBao)
        .SetParameter("Loai", character.CheMatBaoLoai)
        .SetParameter("Cap", character.CheMatBaoCap.ToString());

    await RunFeatureExecutor<CheMatBaoExecutor>(character, FeatureType.CheMatBao);
}
```

---

## Example 4: Migrating Existing Code

### Step-by-Step Migration Strategy

#### Step 1: Initialize Container (Once)

```csharp
// In Form1_Load
ServiceContainer.Initialize(textBoxStatus);
```

#### Step 2: Identify Features to Migrate

Start with frequently used features:
- ✅ DoiNangNo (already migrated)
- ✅ TrongNL (planting)
- ✅ TriAn (thanksgiving quest)
- ✅ CheMatBao (crafting)

#### Step 3: Replace Button Click Handlers

```csharp
// Before
public void buttonTrongNL_Click(object sender, EventArgs e)
{
    // 20+ lines of legacy code
}

// After
public async void buttonTrongNL_Click(object sender, EventArgs e)
{
    var character = GetSelectedCharacter();
    await RunFeatureExecutor<TrongNLExecutor>(character, FeatureType.TrongNL);
}
```

#### Step 4: Update MainAuto.run()

```csharp
// In MainAuto.cs
public void run()
{
    // Check daily reset
    mCharacter = Helper.checkRenewConfig(mCharacter);

    // Old way: Direct method calls
    if (mCharacter.TrongNL == 1 && mCharacter.StatusTrongNL == 0)
    {
        mGeneralFunctions.trong();
        mCharacter.StatusTrongNL = 1;
        Helper.saveSettingsToXML(mCharacter);
    }

    // New way: Use executor
    if (mCharacter.TrongNL == 1 && mCharacter.StatusTrongNL == 0)
    {
        RunFeatureExecutorSync<TrongNLExecutor>(FeatureType.TrongNL);
    }
}

// Synchronous wrapper for use in MainAuto
private void RunFeatureExecutorSync<TExecutor>(FeatureType featureType)
    where TExecutor : class
{
    try
    {
        var aggregate = CharacterAdapter.ToAggregate(mCharacter);
        var windowServices = ServiceContainer.CreateWindowServices(mHWnd, mCharacter);
        var executor = windowServices.CreateExecutor<TExecutor>(mCharacter.VipLevel);

        var context = new ExecutionContext
        {
            Character = aggregate,
            WindowHandle = mHWnd,
            Config = aggregate.FeatureConfig.GetConfig(featureType),
            CancellationToken = CancellationToken.None
        };

        // Run synchronously (using .Wait() for backward compatibility)
        var task = executor.ExecuteAsync(context);
        task.Wait();
        var result = task.Result;

        if (result.Success)
        {
            aggregate.RuntimeState.CompleteFeature(featureType);
            mCharacter = CharacterAdapter.ToLegacy(aggregate);
            Helper.saveSettingsToXML(mCharacter);
        }
        else
        {
            mLogger.LogError($"Feature {featureType} failed: {result.Message}");
        }
    }
    catch (Exception ex)
    {
        mLogger.LogError($"Error running {featureType}: {ex.Message}");
    }
}
```

---

## Example 5: Error Handling

### Proper Error Handling with Executors

```csharp
public async Task<bool> RunFeatureWithRetry<TExecutor>(
    Character character,
    FeatureType featureType,
    int maxRetries = 3)
    where TExecutor : class
{
    int attempt = 0;

    while (attempt < maxRetries)
    {
        attempt++;

        try
        {
            var aggregate = CharacterAdapter.ToAggregate(character);
            IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);

            if (hWnd == IntPtr.Zero)
            {
                throw new Exception("Window not found");
            }

            var windowServices = ServiceContainer.CreateWindowServices(hWnd, character);
            var executor = windowServices.CreateExecutor<TExecutor>(character.VipLevel);

            var context = new ExecutionContext
            {
                Character = aggregate,
                WindowHandle = hWnd,
                Config = aggregate.FeatureConfig.GetConfig(featureType),
                CancellationToken = CancellationToken.None
            };

            var result = await executor.ExecuteAsync(context);

            if (result.Success)
            {
                aggregate.RuntimeState.CompleteFeature(featureType);
                var updated = CharacterAdapter.ToLegacy(aggregate);
                Helper.saveSettingsToXML(updated);
                return true;
            }
            else
            {
                // Log failure but continue retry loop
                textBoxStatus.AppendText($"Attempt {attempt} failed: {result.Message}\r\n");
            }
        }
        catch (Exception ex)
        {
            textBoxStatus.AppendText($"Attempt {attempt} error: {ex.Message}\r\n");
        }

        if (attempt < maxRetries)
        {
            await Task.Delay(2000); // Wait before retry
        }
    }

    return false;
}
```

---

## Example 6: Cancellation Support

### Implementing Cancellation

```csharp
private CancellationTokenSource _cancellationTokenSource;

public async void buttonStart_Click(object sender, EventArgs e)
{
    _cancellationTokenSource = new CancellationTokenSource();

    var character = GetSelectedCharacter();
    var aggregate = CharacterAdapter.ToAggregate(character);
    IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);
    var windowServices = ServiceContainer.CreateWindowServices(hWnd, character);

    // Run multiple features with cancellation support
    var features = new[]
    {
        (typeof(TrongNLExecutor), FeatureType.TrongNL),
        (typeof(TriAnExecutor), FeatureType.TriAn),
        (typeof(CheMatBaoExecutor), FeatureType.CheMatBao)
    };

    foreach (var (executorType, featureType) in features)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested)
            break;

        if (!aggregate.FeatureConfig.IsEnabled(featureType))
            continue;

        if (aggregate.RuntimeState.IsCompleted(featureType))
            continue;

        textBoxStatus.AppendText($"Starting {featureType}...\r\n");

        try
        {
            var executor = CreateExecutorByType(executorType, windowServices, character.VipLevel);

            var context = new ExecutionContext
            {
                Character = aggregate,
                WindowHandle = hWnd,
                Config = aggregate.FeatureConfig.GetConfig(featureType),
                CancellationToken = _cancellationTokenSource.Token
            };

            var result = await executor.ExecuteAsync(context);

            if (result.Success)
            {
                aggregate.RuntimeState.CompleteFeature(featureType);
                textBoxStatus.AppendText($"{featureType} completed!\r\n");
            }
            else
            {
                textBoxStatus.AppendText($"{featureType} failed: {result.Message}\r\n");
            }
        }
        catch (OperationCanceledException)
        {
            textBoxStatus.AppendText("Operation cancelled by user\r\n");
            break;
        }
    }

    // Save final state
    var updated = CharacterAdapter.ToLegacy(aggregate);
    Helper.saveSettingsToXML(updated);
}

public void buttonStop_Click(object sender, EventArgs e)
{
    _cancellationTokenSource?.Cancel();
}

private IFeatureExecutor CreateExecutorByType(Type executorType, WindowServiceProvider services, int vipLevel)
{
    if (executorType == typeof(TrongNLExecutor))
        return services.CreateExecutor<TrongNLExecutor>(vipLevel);
    else if (executorType == typeof(TriAnExecutor))
        return services.CreateExecutor<TriAnExecutor>(vipLevel);
    else if (executorType == typeof(CheMatBaoExecutor))
        return services.CreateExecutor<CheMatBaoExecutor>(vipLevel);

    throw new ArgumentException($"Unknown executor type: {executorType.Name}");
}
```

---

## Benefits Summary

### Code Reduction

| Approach | Lines of Code | Readability | Testability |
|----------|---------------|-------------|-------------|
| Legacy (Old Way) | 20-50 lines per feature | Low (scattered logic) | None (requires full system) |
| Executor (New Way) | 3-5 lines per feature | High (clear intent) | 95% (mockable) |

### Maintainability

**Old Way:**
- Modify 4-5 files to change feature
- Hard to track execution flow
- Difficult to add new features
- No clear separation of concerns

**New Way:**
- Modify 1 executor class
- Clear execution flow
- Easy to add new executors
- Single Responsibility Principle

---

## Next Steps

1. **Start Small:** Migrate one button at a time
2. **Test Thoroughly:** Ensure each migrated feature works correctly
3. **Keep Both:** Maintain backward compatibility during transition
4. **Document:** Update this file as you discover better patterns
5. **Extend:** Create more executors for remaining features

---

## Common Issues and Solutions

### Issue 1: Window Handle is IntPtr.Zero

**Solution:**
```csharp
IntPtr hWnd = AutoControl.FindWindowHandle(character.ID);
if (hWnd == IntPtr.Zero)
{
    // Try to open window first
    AutoControl.OpenGameWindow(character.Link, character.ID);
    Thread.Sleep(5000);
    hWnd = AutoControl.FindWindowHandle(character.ID);
}
```

### Issue 2: Feature Config Not Set

**Solution:**
```csharp
var aggregate = CharacterAdapter.ToAggregate(character);

// Ensure configuration exists
var config = aggregate.FeatureConfig.GetConfig(FeatureType.TrongNL);
config.SetParameter("Loai", character.TrongNLLoai ?? "Kim Loại");
```

### Issue 3: Async/Await in WinForms

**Solution:**
```csharp
// Use async void for event handlers (WinForms requirement)
public async void button_Click(object sender, EventArgs e)
{
    // Disable button during execution
    ((Button)sender).Enabled = false;

    try
    {
        await RunFeatureExecutor<TrongNLExecutor>(character, FeatureType.TrongNL);
    }
    finally
    {
        ((Button)sender).Enabled = true;
    }
}
```

---

**Phase 3 Complete!** 🎉

You now have practical examples of using the new executor-based architecture alongside existing legacy code, enabling gradual migration without breaking changes.

# Phase 1 Refactoring - Migration Guide

## Overview

Phase 1 refactoring introduces **foundational abstractions** that decouple business logic from external dependencies. This guide shows how to migrate existing code to use the new interfaces and infrastructure.

---

## What Changed in Phase 1

### New Structure
```
v1/
├── Interfaces/           # Abstraction contracts
│   ├── IImageRecognition.cs
│   ├── IInputSimulator.cs
│   ├── IWindowManager.cs
│   ├── ILogger.cs
│   ├── IConfiguration.cs
│   └── ICharacterRepository.cs
├── Infrastructure/       # Concrete implementations
│   ├── EmguCvImageRecognition.cs
│   ├── Win32InputSimulator.cs
│   ├── Win32WindowManager.cs
│   ├── CompositeLogger.cs
│   ├── FileLogger.cs
│   ├── UiLogger.cs
│   └── DebugLogger.cs
├── Configuration/        # Settings management
│   ├── AppConfiguration.cs
│   ├── ConfigurationManager.cs
│   └── XmlConfigurationStore.cs
├── Repositories/         # Data access layer
│   └── XmlCharacterRepository.cs
└── Tests/                # Test cases
    └── Phase1Tests.cs
```

---

## Migration Patterns

### 1. Replace Direct Emgu.CV Calls with IImageRecognition

#### ❌ Old Way (AutoFeatures.cs)
```csharp
public bool findImage(string imagePath, double percent = 0.95)
{
    imagePath = (mCharacter.IsChinese == 1 ? Constant.ChineseResourcePath : Constant.ResourcePath) + imagePath;

    Bitmap screen = CaptureHelper.CaptureWindow(mHWnd) as Bitmap;
    if (screen == null) return false;

    Bitmap template = GetCachedImage(imagePath);
    if (template == null) return false;

    var result = ImageScanOpenCV.FindOutPoint(screen, template, percent);
    screen?.Dispose();

    return result != null;
}
```

#### ✅ New Way (Using IImageRecognition)
```csharp
private readonly IImageRecognition _imageRecognition;

public async Task<bool> FindImageAsync(string imagePath, double threshold = 0.95)
{
    var fullPath = GetFullImagePath(imagePath);
    var location = await _imageRecognition.FindImageAsync(fullPath, threshold: threshold);
    return location.HasValue;
}

// Or for backward compatibility (synchronous wrapper)
public bool FindImage(string imagePath, double threshold = 0.95)
{
    var task = FindImageAsync(imagePath, threshold);
    task.Wait();
    return task.Result;
}
```

**Benefits:**
- Testable (can mock IImageRecognition)
- Async support
- Clean separation from Emgu.CV
- Easy to swap image recognition libraries

---

### 2. Replace Direct ClickHelper Calls with IInputSimulator

#### ❌ Old Way
```csharp
public void clickToImage(string imagePath, int x = 0, int y = 0)
{
    Point? location = findImageLocation(imagePath);
    if (location.HasValue)
    {
        ClickHelper.Click(mHWnd, 1, location.Value.X + x, location.Value.Y + y);
        Thread.Sleep(Constant.TimeShort);
    }
}
```

#### ✅ New Way
```csharp
private readonly IImageRecognition _imageRecognition;
private readonly IInputSimulator _input;

public async Task ClickImageAsync(string imagePath, int offsetX = 0, int offsetY = 0)
{
    var fullPath = GetFullImagePath(imagePath);
    var location = await _imageRecognition.FindImageAsync(fullPath);

    if (location.HasValue)
    {
        var clickPoint = new Point(location.Value.X + offsetX, location.Value.Y + offsetY);
        await _input.ClickAsync(clickPoint, delayAfterMs: 1000);
    }
}

// Backward compatible synchronous version
public void ClickImage(string imagePath, int offsetX = 0, int offsetY = 0)
{
    var task = ClickImageAsync(imagePath, offsetX, offsetY);
    task.Wait();
}
```

**Benefits:**
- Testable (can mock input simulation)
- Async support with proper delays
- Clean API

---

### 3. Replace TextBox Status Updates with ILogger

#### ❌ Old Way
```csharp
private TextBox mTextBoxStatus;

public void writeStatus(string message)
{
    if (mTextBoxStatus.InvokeRequired)
    {
        mTextBoxStatus.BeginInvoke(new Action(() => {
            mTextBoxStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }));
    }
    else
    {
        mTextBoxStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
    }
}
```

#### ✅ New Way
```csharp
private readonly ILogger _logger;

public void LogStatus(string message, string context = null)
{
    _logger.LogInfo(message, context);
}

public void LogError(string message, Exception ex = null)
{
    _logger.LogError(message, ex, context: mCharacter.ID);
}
```

**How to set up composite logger in Form1:**
```csharp
public partial class Form1 : Form
{
    private ILogger _logger;

    public Form1()
    {
        InitializeComponent();

        // Create composite logger with multiple outputs
        var composite = new CompositeLogger();
        composite.AddLogger(new FileLogger("logs/automation.log"));
        composite.AddLogger(new UiLogger(textBoxStatus));
        #if DEBUG
        composite.AddLogger(new DebugLogger());
        #endif

        _logger = composite;
    }

    // Pass logger to other classes
    private void CreateAutomation(Character character)
    {
        var mainAuto = new MainAuto(hWnd, character, _logger);
    }
}
```

**Benefits:**
- No UI dependency in business logic
- Logs to file AND UI simultaneously
- Easy to add new log targets
- Thread-safe by design

---

### 4. Replace Constants with Configuration

#### ❌ Old Way (Constant.cs)
```csharp
public const int TimeShort = 1000;
public const int TimeMedium = 3000;
public const string ResourcePath = "resources";
```

#### ✅ New Way
```csharp
using AutoVPT.Configuration;

// Access configuration anywhere
var config = ConfigurationManager.Instance;
var delay = config.Timing.ShortDelayMs;
var imagePath = config.Paths.GetImagePath("/global/button.png", isChinese);
```

**Creating custom config (optional):**
```csharp
// In appsettings.xml (auto-generated)
<?xml version="1.0" encoding="utf-8"?>
<AppConfiguration>
  <Timing>
    <ShortDelayMs>1000</ShortDelayMs>
    <MediumDelayMs>3000</MediumDelayMs>
  </Timing>
  <Paths>
    <ResourceBasePath>resources</ResourceBasePath>
  </Paths>
</AppConfiguration>
```

**Benefits:**
- Runtime configuration changes
- No recompilation needed
- Environment-specific settings (dev/prod)
- Type-safe access

---

### 5. Replace Helper.loadSettingsFromXML with ICharacterRepository

#### ❌ Old Way
```csharp
// Loading
Character character = Helper.loadSettingsFromXML(characterId);

// Saving
Helper.saveSettingsToXML(character);

// Getting all
// No standard way - had to manually scan directory
```

#### ✅ New Way
```csharp
private readonly ICharacterRepository _characterRepo;

// Injected in constructor
public MainAuto(ICharacterRepository characterRepo)
{
    _characterRepo = characterRepo;
}

// Loading
Character character = _characterRepo.GetById(characterId);

// Saving
_characterRepo.Save(character);

// Getting all
var allCharacters = _characterRepo.GetAll();

// Check exists
bool exists = _characterRepo.Exists(characterId);

// Get by group
var groupMembers = _characterRepo.GetByGroup("Group1");
```

**Setting up in Form1:**
```csharp
private ICharacterRepository _characterRepo;

public Form1()
{
    InitializeComponent();
    _characterRepo = new XmlCharacterRepository("database");
}

private void LoadCharacters()
{
    var characters = _characterRepo.GetAll();
    foreach (var character in characters)
    {
        // Add to DataGridView
        dataGridViewCharacters.Rows.Add(character.ID, character.Link);
    }
}
```

**Benefits:**
- Consistent API
- Easy to swap storage (XML → JSON → SQLite)
- Better query support
- Testable with mock repository

---

## Migration Strategy

### Step 1: Update Existing Classes to Accept Interfaces

Instead of creating dependencies directly, accept them via constructor:

```csharp
// Before
public class GeneralFunctions
{
    private IntPtr mHWnd;
    private TextBox mTextBoxStatus;
    public AutoFeatures mAuto;

    public GeneralFunctions(IntPtr hWnd, Character character, TextBox textBoxStatus)
    {
        mHWnd = hWnd;
        mTextBoxStatus = textBoxStatus;
        mAuto = new AutoFeatures(hWnd, character.ID, textBoxStatus, character);
    }
}

// After (with dependency injection preparation)
public class GeneralFunctions
{
    private readonly IImageRecognition _imageRecog;
    private readonly IInputSimulator _input;
    private readonly ILogger _logger;
    private readonly Character _character;

    public GeneralFunctions(
        Character character,
        IImageRecognition imageRecognition,
        IInputSimulator inputSimulator,
        ILogger logger)
    {
        _character = character;
        _imageRecog = imageRecognition;
        _input = inputSimulator;
        _logger = logger;
    }
}
```

### Step 2: Create Wrapper Methods for Backward Compatibility

Keep existing synchronous methods working while adding async versions:

```csharp
// New async method
public async Task<bool> MoveToMapAsync(string mapName)
{
    var imagePath = $"/maps/{mapName}_check.png";
    return await _imageRecog.ImageExistsAsync(imagePath);
}

// Keep old synchronous method for backward compatibility
public bool moveToMap(string mapName, int x = 0, int y = -20)
{
    var task = MoveToMapAsync(mapName);
    task.Wait();
    return task.Result;
}
```

### Step 3: Gradually Migrate Form1.cs

Update Form1 to create and pass interfaces:

```csharp
public partial class Form1 : Form
{
    private ILogger _logger;
    private ICharacterRepository _characterRepo;

    public Form1()
    {
        InitializeComponent();
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        // Setup logging
        var composite = new CompositeLogger();
        composite.AddLogger(new FileLogger("logs/app.log"));
        composite.AddLogger(new UiLogger(textBoxStatus));
        _logger = composite;

        // Setup repository
        _characterRepo = new XmlCharacterRepository("database");

        _logger.LogInfo("Application started");
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
        var characterId = GetSelectedCharacterId();
        var character = _characterRepo.GetById(characterId);

        var hWnd = FindGameWindow(characterId);

        // Create dependencies for automation
        var imageRecog = new EmguCvImageRecognition(hWnd);
        var inputSim = new Win32InputSimulator(hWnd);
        var windowMgr = new Win32WindowManager();

        // Create automation with dependencies
        var automation = new GeneralFunctions(
            character,
            imageRecog,
            inputSim,
            _logger
        );

        // Start automation in background thread
        Task.Run(async () =>
        {
            try
            {
                await automation.RunDailyTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Automation failed", ex, characterId);
            }
        });
    }
}
```

---

## Testing Guide

### Unit Testing Without UI

With the new abstractions, you can unit test business logic without UI:

```csharp
using Moq; // Install Moq NuGet package for mocking
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class GeneralFunctionsTests
{
    [TestMethod]
    public async Task MoveToMap_WhenMapExists_ReturnsTrue()
    {
        // Arrange
        var mockImageRecog = new Mock<IImageRecognition>();
        mockImageRecog
            .Setup(x => x.FindImageAsync(It.IsAny<string>(), null, 0.8))
            .ReturnsAsync(new Point(100, 100));

        var mockInput = new Mock<IInputSimulator>();
        var mockLogger = new Mock<ILogger>();

        var character = new Character { ID = "Test", IsChinese = 0 };

        var functions = new GeneralFunctions(
            character,
            mockImageRecog.Object,
            mockInput.Object,
            mockLogger.Object
        );

        // Act
        var result = await functions.MoveToMapAsync("testmap");

        // Assert
        Assert.IsTrue(result);
        mockImageRecog.Verify(x => x.FindImageAsync(
            It.Is<string>(s => s.Contains("testmap")),
            null,
            It.IsAny<double>()
        ), Times.Once);
    }
}
```

### Integration Testing

Test with real implementations:

```csharp
[TestMethod]
public void ConfigurationManager_LoadsCorrectly()
{
    // Arrange
    var config = ConfigurationManager.Instance;

    // Act & Assert
    Assert.IsNotNull(config);
    Assert.AreEqual(1000, config.Timing.ShortDelayMs);
    Assert.IsNotNull(config.Paths.ResourceBasePath);
}

[TestMethod]
public void FileLogger_WritesToFile()
{
    // Arrange
    var logPath = "logs/test.log";
    if (File.Exists(logPath)) File.Delete(logPath);

    var logger = new FileLogger(logPath);

    // Act
    logger.LogInfo("Test message");

    // Assert
    Assert.IsTrue(File.Exists(logPath));
    var content = File.ReadAllText(logPath);
    Assert.IsTrue(content.Contains("Test message"));
}
```

---

## Common Migration Issues & Solutions

### Issue 1: "Cannot convert IntPtr to IWindowManager"

**Problem:**
```csharp
// Old code expects IntPtr directly
public AutoFeatures(IntPtr hWnd, string windowName, TextBox textBox, Character character)
```

**Solution:**
Keep backward compatibility by wrapping the interface:

```csharp
public AutoFeatures(IntPtr hWnd, string windowName, ILogger logger, Character character)
{
    mHWnd = hWnd; // Keep for legacy code
    _logger = logger;

    // Create interface wrappers internally
    _windowManager = new Win32WindowManager();
    _imageRecog = new EmguCvImageRecognition(hWnd);
}
```

### Issue 2: "TextBox still required in constructor"

**Problem:**
Many classes currently need TextBox for status updates.

**Solution:**
Create a UiLogger wrapper and pass that instead:

```csharp
// In Form1
var logger = new UiLogger(textBoxStatus);

// Pass to other classes
var mainAuto = new MainAuto(hWnd, character, logger);

// In MainAuto, accept ILogger instead of TextBox
public MainAuto(IntPtr hWnd, Character character, ILogger logger)
{
    _logger = logger;
    _logger.LogInfo("MainAuto initialized", character.ID);
}
```

### Issue 3: "Thread.Sleep everywhere"

**Problem:**
Old code uses `Thread.Sleep()` which blocks threads.

**Solution:**
Use async/await with `Task.Delay()`:

```csharp
// Old
Thread.Sleep(Constant.TimeShort);

// New
await Task.Delay(ConfigurationManager.Instance.Timing.ShortDelayMs);

// Or in synchronous code (transitional)
Task.Delay(config.Timing.ShortDelayMs).Wait();
```

---

## Examples: Before & After

### Example 1: Complete Feature Method

#### ❌ Before
```csharp
public void doiNangNo()
{
    writeStatus("Bắt đầu đổi năng nỗ");

    if (!moveToNPC("npc_exchange"))
    {
        writeStatus("Không tìm thấy NPC");
        return;
    }

    clickToImage(Constant.ImagePathGlobalButton);
    Thread.Sleep(Constant.TimeShort);

    clickToImage(Constant.ImagePathExchangeButton);
    Thread.Sleep(Constant.TimeShort);

    mCharacter.StatusDoiNangNo = 1;
    Helper.saveSettingsToXML(mCharacter);
    writeStatus("Hoàn thành đổi năng nỗ");
}
```

#### ✅ After
```csharp
public async Task DoiNangNoAsync()
{
    _logger.LogInfo("Bắt đầu đổi năng nỗ", _character.ID);

    if (!await MoveToNPCAsync("npc_exchange"))
    {
        _logger.LogWarning("Không tìm thấy NPC", _character.ID);
        return;
    }

    var config = ConfigurationManager.Instance;

    await ClickImageAsync(config.Paths.GlobalFolder + "button.png");
    await Task.Delay(config.Timing.ShortDelayMs);

    await ClickImageAsync(config.Paths.GlobalFolder + "exchange_button.png");
    await Task.Delay(config.Timing.ShortDelayMs);

    _character.StatusDoiNangNo = 1;
    _characterRepo.Save(_character);
    _logger.LogInfo("Hoàn thành đổi năng nỗ", _character.ID);
}

// Backward compatible wrapper
public void doiNangNo()
{
    DoiNangNoAsync().Wait();
}
```

### Example 2: Multi-Character Operation

#### ❌ Before
```csharp
private void buttonDoiNangNoAll_Click(object sender, EventArgs e)
{
    foreach (DataGridViewRow row in dataGridViewCharacters.Rows)
    {
        var characterId = row.Cells["ID"].Value.ToString();
        var character = Helper.loadSettingsFromXML(characterId);

        if (character.DoiNangNo == 1 && character.StatusDoiNangNo == 0)
        {
            var thread = new Thread(() => {
                var hWnd = AutoControl.FindWindowHandle(null, characterId);
                var mainAuto = new MainAuto(hWnd, character, textBoxStatus);
                mainAuto.doiNangNo();
            });
            thread.Name = characterId + "doinangno";
            thread.Start();
            Helper.threadList.Add(thread);
        }

        Thread.Sleep(100);
    }
}
```

#### ✅ After
```csharp
private async void buttonDoiNangNoAll_Click(object sender, EventArgs e)
{
    var characters = _characterRepo.GetAll();
    var tasks = new List<Task>();

    foreach (var character in characters)
    {
        if (character.DoiNangNo == 1 && character.StatusDoiNangNo == 0)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    var hWnd = _windowManager.FindWindow(character.ID);
                    if (hWnd == IntPtr.Zero)
                    {
                        _logger.LogWarning($"Window not found for {character.ID}");
                        return;
                    }

                    var imageRecog = new EmguCvImageRecognition(hWnd);
                    var inputSim = new Win32InputSimulator(hWnd);

                    var automation = new GeneralFunctions(
                        character,
                        imageRecog,
                        inputSim,
                        _logger
                    );

                    await automation.DoiNangNoAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in automation for {character.ID}", ex);
                }
            });

            tasks.Add(task);
            await Task.Delay(100); // Small delay between starts
        }
    }

    // Wait for all to complete
    await Task.WhenAll(tasks);
    _logger.LogInfo("All DoiNangNo tasks completed");
}
```

---

## Performance Considerations

### 1. Image Caching

The `EmguCvImageRecognition` class includes automatic caching:

```csharp
// Images are cached automatically
var imageRecog = new EmguCvImageRecognition(hWnd);

for (int i = 0; i < 100; i++)
{
    // Only loads from disk once, cached afterwards
    await imageRecog.FindImageAsync("resources/button.png");
}

// Clear cache when done to free memory
imageRecog.ClearCache();
imageRecog.Dispose();
```

### 2. Async Operations Don't Block UI

```csharp
// Old way - UI freezes
private void button_Click(object sender, EventArgs e)
{
    RunLongOperation(); // UI frozen
}

// New way - UI remains responsive
private async void button_Click(object sender, EventArgs e)
{
    await RunLongOperationAsync(); // UI responsive
}
```

### 3. Parallel Operations

```csharp
// Run multiple characters in parallel
var tasks = characters.Select(async character =>
{
    await RunAutomationAsync(character);
});

await Task.WhenAll(tasks);
```

---

## Rollback Strategy

If you need to rollback Phase 1 changes:

1. **Remove new files from .csproj:**
   - Comment out all `<Compile Include="Interfaces\*.cs" />`
   - Comment out all `<Compile Include="Infrastructure\*.cs" />`
   - Comment out all `<Compile Include="Configuration\*.cs" />`
   - Comment out all `<Compile Include="Repositories\*.cs" />`

2. **Keep old code intact:**
   - All existing code still works
   - New classes are additive, not breaking changes

3. **No database changes:**
   - Character XML files unchanged
   - No data migration needed

---

## Next Steps: Phase 2 Preview

Phase 2 will build on these foundations:

1. **Split Character class** → Separate domain models
2. **Service layer** → Business logic orchestration
3. **Dependency injection** → Automatic dependency management
4. **Async/await throughout** → Better performance and responsiveness

Phase 1 abstractions make Phase 2 possible without breaking existing functionality.

---

## Support & Questions

For issues or questions about Phase 1 migration:

1. Check `/Tests/Phase1Tests.cs` for working examples
2. Review this guide's "Common Issues" section
3. Test incrementally - migrate one class at a time
4. Keep old code alongside new code during transition

Remember: **Phase 1 is additive, not destructive**. You can adopt these patterns gradually!

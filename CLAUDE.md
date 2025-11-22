# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**McHelper (AutoVPT)** is a C# Windows Forms automation tool for a game. The application automates various in-game tasks including resource gathering, daily quests, dungeon runs, and character management. It uses image recognition (via Emgu.CV) to interact with Adobe Flash Player windows.

## Build and Development Commands

### Building the Project
```bash
# Build Debug configuration (x86)
cd /mnt/c/Users/ADMIN/source/repos/McHelper/v1
msbuild AutoVPT.sln /p:Configuration=Debug /p:Platform=x86

# Build Release configuration (x86)
msbuild AutoVPT.sln /p:Configuration=Release /p:Platform=x86
```

### Opening in Visual Studio
```bash
# Open the solution
start AutoVPT.sln
```

**Note:** This project is configured for x86 platform specifically to interact with 32-bit Flash Player processes.

## Architecture

### Core Components

1. **MainForm (Form1.cs)** - Primary UI that manages:
   - Character list (DataGridView) loaded from XML database
   - Configuration panels for automation settings per character
   - Status text box for logging automation events
   - Buttons to trigger individual or batch automation tasks

2. **MainAuto (MainAuto.cs)** - Orchestration layer that:
   - Coordinates the main automation loop (`run()` method)
   - Ensures game window is open and logged in before executing tasks
   - Calls feature-specific methods from `GeneralFunctions`
   - Manages feature status tracking (prevents duplicate runs of daily tasks)
   - Runs each automation action in a separate thread via `Helper.threadList`

3. **GeneralFunctions (GeneralFunctions.cs)** - Implements specific game automation features:
   - Daily task automation (VIP rewards, dungeons, quests)
   - Resource management (planting materials, crafting)
   - Combat automation (monster hunting, pet battles)
   - Navigation and NPC interaction

4. **AutoFeatures (AutoFeatures.cs)** - Low-level automation primitives:
   - Image recognition using Emgu.CV
   - Mouse/keyboard input via KAutoHelper.dll
   - Window management (finding/controlling Flash Player windows)
   - Movement and battle detection

### Data Model

- **Character class (Objects/Character.cs)** - 75+ properties tracking:
  - Basic info: ID, Link (game URL), Group (for party features)
  - Feature enablement flags (e.g., `DoiNangNo`, `TrongNL`, `AutoPhuBan`)
  - Feature status tracking (e.g., `StatusDoiNangNo` - 0=not done, 1=completed today)
  - Configuration (e.g., `CheMatBaoLoai`, `TrongNLLoai` - which resources to use)

- **Character persistence** - XML serialization in `database/` folder via `Helper.cs`
  - Each character stored as `{CharacterID}.xml`
  - Settings reset daily based on `Date` property comparison

### Feature Status System

Features use a status tracking pattern to prevent duplicate execution:
- `character.{Feature}` = 1 → feature enabled in settings
- `character.Status{Feature}` = 0 → not yet run today
- When feature completes, `Status{Feature}` set to 1 and saved to XML
- Status fields reset to 0 when date changes (see `checkRenewConfig()`)

### Threading Architecture

- Each automation action runs in a separate thread via `Helper.threadList`
- Thread names format: `{CharacterID}{actionName}` (e.g., "Character1mainauto")
- `character.Running` flag: 0=stopped, 1=normal auto, 2=event mode
- Main form provides "Stop" and "Stop All" buttons that abort threads by name

### Window Management

The application interacts with Flash Player instances:
- Opens `flash.exe` with game URL per character
- Renames window title to `character.ID` for identification
- Uses `AutoControl.FindWindowHandle()` to locate windows
- Supports VPN binding via `ForceBindIP.exe` for Chinese server (`IsChinese` flag)

### Image Recognition System

Images organized by feature in `/resources/` (or `/cn_resources/` for Chinese version):
- `/global/` - UI elements (minimap, buttons, dialogs)
- `/phu_ban/` - Dungeon-specific images
- `/mat_bao/` - Crafting materials
- `/tru_ma/` - Monster hunting
- `/trong_nl/` - Resource planting
- `/tri_an/` - Quest system
- `/in_map/`, `/maps/` - Navigation

Key methods in `AutoFeatures.cs`:
- `findImage()` / `findImageByGroup()` - Locate images on screen
- `clickToImage()` / `clickImageByGroup()` - Click found images
- `isMoving()` / `dangTrongTranDau()` - State detection

## Common Development Patterns

### Adding a New Automation Feature

1. Add properties to `Character` class:
   - Boolean flag (e.g., `int new_feature`)
   - Configuration properties (e.g., `string new_feature_type`)
   - Status property (e.g., `int status_new_feature`)

2. Add UI controls to `Form1.Designer.cs`:
   - CheckBox for enable/disable
   - ComboBox/NumericUpDown for configuration
   - Status indicator checkbox

3. Implement feature logic in `GeneralFunctions.cs`:
   ```csharp
   public void newFeature() {
       // Navigation, image recognition, interaction logic
   }
   ```

4. Add wrapper in `MainAuto.cs`:
   ```csharp
   public void newFeature() {
       runAction("newFeature", () => mGeneralFunctions.newFeature());
   }
   ```

5. Integrate into main loop in `MainAuto.run()`:
   ```csharp
   if (mCharacter.NewFeature == 1 && mCharacter.StatusNewFeature == 0) {
       mGeneralFunctions.newFeature();
       mCharacter.StatusNewFeature = 1;
       Helper.saveSettingsToXML(mCharacter);
   }
   ```

### Image Recognition Best Practices

- Store images in appropriate subfolder under `/resources/`
- Use descriptive filenames (e.g., `button_confirm.png`, not `img1.png`)
- For Chinese server support, duplicate to `/cn_resources/` with translated text
- Use `Constant.cs` for image path constants
- Test image recognition with `captureImage()` method to verify coordinates

### Working with Character Settings

Loading:
```csharp
character = Helper.loadSettingsFromXML(characterId);
```

Saving:
```csharp
Helper.saveSettingsToXML(character);
```

UI binding pattern in `Form1.cs`:
```csharp
// Load settings to UI
checkBoxFeature.Checked = (character.Feature >= 1);
comboBoxType.SelectedIndex = comboBoxType.FindStringExact(character.FeatureLoai);

// Save UI to settings (in parsingAndUpdateCharacter)
character.Feature = (this.checkBoxFeature.Checked) ? 1 : 0;
character.FeatureLoai = this.comboBoxType.Text;
```

## Key Constants

Located in `Constant.cs`:
- `Version` - Application version string
- Timing: `VeryTimeShort` (100ms), `TimeShort` (1s), `TimeMedium` (3s), `TimeLong` (5s)
- Status: `StatusFeatureInactive` (0), `StatusFeatureActive` (1), `StatusFeatureRunning` (2), `StatusFeatureCompleted` (3)
- Image paths organized by category (all relative to resource folder)

## Dependencies

- **.NET Framework 4.7.2** - Target framework
- **KAutoHelper.dll** - Native library for mouse/keyboard automation
- **Emgu.CV.World.dll** - Computer vision (OpenCV wrapper for .NET)
- **ZedGraph** (NuGet) - Graphing library (version 5.1.7)
- **Flash Player components** - ShockwaveFlashObjects interop
- **ForceBindIP.exe** - VPN binding utility for multi-region support

## Multi-Character and Batch Operations

The UI provides "All" buttons that iterate through `dataGridViewCharacters.Rows` to run features across multiple characters:
- Each character gets its own thread with delay (`Thread.Sleep(Constant.VeryTimeShort)`)
- Pattern: load character → get window handle → create `MainAuto` → run in thread
- Example: `buttonDoiNangNoAll_Click` runs resource exchange for all active characters

Group features:
- Characters with matching `Group` property can form parties
- `taoNhom()` invites members, `dongYVaoNhom()` accepts invites
- Party coordination for dungeons and quests

## Important Notes

- **x86 Platform Required** - Must build as 32-bit to interact with 32-bit Flash Player
- **Thread Safety** - UI updates use `textBox.BeginInvoke()` for cross-thread operations
- **State Management** - Always check `character.Running` before starting automation
- **Daily Reset** - Status flags automatically reset when date changes
- **Chinese Server** - Set `IsChinese=1` to use ForceBindIP and cn_resources folder

# Migration Checklist

Quick guide to apply the improvements to your codebase.

## ✅ Already Applied (Automatic)

These improvements are already working:

- ✅ Image caching in AutoFeatures
- ✅ CancellationToken in stop buttons
- ✅ Error logging throughout codebase
- ✅ Process tracking in openWindow()
- ✅ Rate limiting example in buttonDoiNangNoAll_Click

## 🔧 Manual Steps Required

### Step 1: Add Form Closing Handler

Add this to `Form1.cs`:

```csharp
private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
{
    try
    {
        // Clean up all game processes
        GeneralFunctions.CloseAllWindows();

        // Flush pending character saves
        CharacterSettingsManager.Instance.FlushAll();

        // Cleanup rate limiter
        batchRateLimiter?.Dispose();

        // Clean old logs (keep last 7 days)
        Logger.ClearOldLogs(7);

        // Cancel any running operations
        var allKeys = Helper.cancellationTokens.Keys.ToList();
        foreach (var key in allKeys)
        {
            Helper.CancelToken(key);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError("System", "MainForm_FormClosing", ex);
    }
}
```

Then add the event handler in Form Designer or constructor:
```csharp
public MainForm()
{
    InitializeComponent();
    this.FormClosing += MainForm_FormClosing;
}
```

### Step 2: Add Rate Limiting to Other "All" Operations

Find methods like:
- `buttonDaPetAll_Click`
- `buttonAoMaAll_Click`
- `buttonNhanHoiPhucAll_Click`
- `buttonDoiNangNoAll_Click` (already done)
- `buttonNhanThuongAutoPBAll_Click`
- `buttonCheMbAll_Click`
- `buttonTrongNLAll_Click`
- etc.

Wrap the operations in `batchRateLimiter.Execute()`:

```csharp
private void button{Action}All_Click(object sender, EventArgs e)
{
    foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
    {
        var charId = item.Cells[0].Value.ToString();

        batchRateLimiter.Execute(() =>
        {
            character = Helper.loadSettingsFromXML(charId);

            if (character.ID != null && character.ID != "" && checkWindowOpen())
            {
                // Your existing code here
            }
        });
    }
}
```

### Step 3: Gradually Migrate to CharacterSettingsManager (Optional)

Find places where you call:
```csharp
Helper.saveSettingsToXML(character);
```

Consider replacing with:
```csharp
CharacterSettingsManager.Instance.MarkDirty(character);
```

**Priority locations:**
1. Inside loops (high frequency saves)
2. In `MainAuto.run()` method
3. In `parsingAndUpdateCharacter()`

**Keep as-is for now:**
- Final saves before app exit
- Critical saves where you need immediate persistence

### Step 4: Build and Test

```bash
cd v1
msbuild AutoVPT.sln /p:Configuration=Debug /p:Platform=x86
```

Check for compilation errors:
- Missing using statements
- Namespace issues
- Missing references

### Step 5: Add to .csproj File

Ensure these new files are included in `AutoVPT.csproj`:

```xml
<Compile Include="Libs\Logger.cs" />
<Compile Include="Libs\CharacterSettingsManager.cs" />
<Compile Include="Libs\RateLimiter.cs" />
```

If using Visual Studio, right-click project → Add → Existing Item

## 🧪 Testing Checklist

### Basic Functionality
- [ ] Application starts without errors
- [ ] Can select characters
- [ ] Can open game windows
- [ ] Can run automation

### Stop Functionality
- [ ] Stop button works (graceful stop)
- [ ] Stop All button works
- [ ] No error messages when stopping
- [ ] No orphaned threads

### Memory & Performance
- [ ] Memory usage stable (not growing)
- [ ] Image recognition fast
- [ ] No flash.exe processes after closing
- [ ] `/logs/` folder created and populated

### Batch Operations
- [ ] "All" buttons work
- [ ] Only 3 operations run at once (rate limiting)
- [ ] No system overload
- [ ] All characters processed

### File Operations
- [ ] Character settings save/load
- [ ] XML files not corrupted
- [ ] Tracking folder exists
- [ ] Database folder accessible

## 📊 Monitoring

### Check Logs
Look in `/logs/YYYY-MM-DD.log` for:
- Startup messages
- Error patterns
- Warning messages

### Monitor Resources
Use Task Manager to verify:
- Memory usage stable
- CPU usage reasonable
- No process leaks

### Verify Improvements
Compare before/after:
- Memory footprint
- Automation speed
- Stop button responsiveness
- File I/O activity

## ⚠️ Common Issues

### Issue: Build Errors

**Missing namespace imports:**
```csharp
using AutoVPT.Libs;
using System.Threading;
```

**Files not included:**
- Right-click project → Add Existing Item
- Select Logger.cs, CharacterSettingsManager.cs, RateLimiter.cs

### Issue: Stop Button Not Working

**Check:**
1. CancellationToken setup in Helper.cs
2. runAction() method updated in MainAuto.cs
3. Button click handlers updated in Form1.cs

### Issue: Rate Limiting Not Working

**Verify:**
1. RateLimiter instance created in MainForm
2. Execute() wrapper added to button handler
3. No early returns before Execute()

### Issue: Logs Not Created

**Check:**
1. Application has write permissions
2. Logger.cs is compiled and included
3. No exceptions thrown during Logger initialization

## 🎯 Priority Order

If time-limited, implement in this order:

1. **Critical** (do first):
   - Form closing handler
   - Verify stop buttons work

2. **High Impact** (do soon):
   - Rate limiting for "All" operations
   - CharacterSettingsManager migration

3. **Nice to Have** (when time permits):
   - Review logs for patterns
   - Optimize specific bottlenecks
   - Add telemetry/metrics

## 📝 Notes

- **No Breaking Changes**: Old code still works
- **Gradual Migration**: Can adopt improvements incrementally
- **Backwards Compatible**: Existing character XML files unchanged
- **Safe to Revert**: All new files can be removed if needed

## ✅ Completion Checklist

- [ ] All new .cs files added to project
- [ ] Build succeeds with no errors
- [ ] Form closing handler added
- [ ] Rate limiting added to "All" operations
- [ ] Tested basic automation flow
- [ ] Tested stop functionality
- [ ] Verified no memory leaks
- [ ] Checked logs for errors
- [ ] No orphaned processes after close
- [ ] Documentation read and understood

---

**Questions or issues?** Check IMPROVEMENTS.md for detailed explanations.

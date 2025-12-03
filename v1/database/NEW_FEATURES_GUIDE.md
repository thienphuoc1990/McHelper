# New SQLite Features Integration Guide

## Overview

After migrating to SQLite, three new powerful features are available:

1. **Reporting Dashboard** - Full-featured dashboard showing character progress
2. **Statistics Panel** - Compact widget for main form sidebar
3. **Historical Tracking** - Track task completion over time with analytics

## Feature 1: Reporting Dashboard

### What It Does
- Shows overall progress across all characters
- Lists characters with incomplete tasks
- Filters by group
- Displays per-feature statistics

### How to Integrate

**Add menu item or button to main form:**

```csharp
using AutoVPT.UI;

// In Form1.cs - add button click handler
private void buttonShowDashboard_Click(object sender, EventArgs e)
{
    var dashboard = new ReportingDashboard();
    dashboard.ShowDialog(this);
}
```

**Or add to main menu:**

```csharp
// In Form1.Designer.cs or Form1.cs
var menuItemReports = new ToolStripMenuItem("Reports");
var menuItemDashboard = new ToolStripMenuItem("View Dashboard", null, (s, e) =>
{
    var dashboard = new ReportingDashboard();
    dashboard.ShowDialog(this);
});
menuItemReports.DropDownItems.Add(menuItemDashboard);
mainMenuStrip.Items.Add(menuItemReports);
```

### Dashboard Tabs

1. **Overview** - Total characters, completion rate, progress bar
2. **Incomplete Tasks** - List of characters with pending tasks
3. **By Group** - Filter and view by character group
4. **Statistics** - Per-feature completion statistics

## Feature 2: Statistics Panel

### What It Does
- Compact overview widget (300x180 pixels)
- Shows total/completed/incomplete counts
- Progress bar with percentage
- Auto-refreshes every 30 seconds
- "View Details" button to open full dashboard

### How to Integrate

**Add to main form sidebar:**

```csharp
using AutoVPT.UI;

// In Form1.cs or Form1.Designer.cs
private StatisticsPanel statisticsPanel;

private void InitializeStatisticsPanel()
{
    statisticsPanel = new StatisticsPanel
    {
        Location = new Point(10, 10), // Adjust to your layout
        Dock = DockStyle.None
    };

    // Handle "View Details" button click
    statisticsPanel.ViewDetailsClicked += (s, e) =>
    {
        var dashboard = new ReportingDashboard();
        dashboard.ShowDialog(this);
    };

    this.Controls.Add(statisticsPanel);
}

// In Form1_Load
private void Form1_Load(object sender, EventArgs e)
{
    DatabaseHelper.Initialize();
    HistoryTracker.Initialize();

    InitializeStatisticsPanel();

    // Your existing code...
}

// Optional: Refresh when characters are updated
private void RefreshStatistics()
{
    statisticsPanel?.LoadStatistics();
}
```

**Positioning Options:**

```csharp
// Option 1: Top-right corner
statisticsPanel.Location = new Point(this.Width - 320, 10);
statisticsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

// Option 2: Left sidebar
statisticsPanel.Location = new Point(10, 100);
statisticsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

// Option 3: Panel container
var panelContainer = new Panel { Dock = DockStyle.Right, Width = 320 };
statisticsPanel.Dock = DockStyle.Top;
panelContainer.Controls.Add(statisticsPanel);
this.Controls.Add(panelContainer);
```

## Feature 3: Historical Tracking

### What It Does
- Records every task completion with timestamp
- Tracks daily summary statistics
- Provides analytics views (trends, popular tasks, performance)
- Automatic cleanup of old data

### How to Integrate

**Step 1: Initialize on Application Startup**

```csharp
using AutoVPT.Database;

// In Form1_Load or Application startup
private void Form1_Load(object sender, EventArgs e)
{
    // Initialize database
    DatabaseHelper.Initialize();

    // Initialize history tracking
    HistoryTracker.Initialize();

    // Your existing code...
}
```

**Step 2: Record Task Completions**

When a task status changes from 0 to 1, record it:

```csharp
// Example: In MainAuto.cs or wherever task completion happens

// Single task completion
character.StatusDoiNangNo = 1;
Helper.saveSettingsToXML(character);
HistoryTracker.RecordTaskCompletion(character.ID, "DoiNangNo");

// Multiple tasks at once
var completedTasks = new List<string>();
if (character.StatusDoiNangNo == 0) completedTasks.Add("DoiNangNo");
if (character.StatusTriAn == 0) completedTasks.Add("TriAn");

character.StatusDoiNangNo = 1;
character.StatusTriAn = 1;
Helper.saveSettingsToXML(character);

if (completedTasks.Count > 0)
{
    HistoryTracker.RecordTaskCompletions(character.ID, completedTasks);
}
```

**Step 3: Update Daily Summary**

Update summary at end of day or on demand:

```csharp
// Option 1: Manual trigger (button)
private void buttonUpdateSummary_Click(object sender, EventArgs e)
{
    HistoryTracker.UpdateDailySummary();
    MessageBox.Show("Daily summary updated!");
}

// Option 2: Automatic at end of "Auto All"
private void AllAutomationComplete()
{
    HistoryTracker.UpdateDailySummary();
}

// Option 3: Timer-based (end of day)
private Timer dailySummaryTimer;

private void SetupDailySummaryTimer()
{
    dailySummaryTimer = new Timer();
    dailySummaryTimer.Interval = 3600000; // Check every hour

    dailySummaryTimer.Tick += (s, e) =>
    {
        var now = DateTime.Now;
        // Update summary at 11:59 PM
        if (now.Hour == 23 && now.Minute >= 59)
        {
            HistoryTracker.UpdateDailySummary();
        }
    };

    dailySummaryTimer.Start();
}
```

**Step 4: View Historical Data**

```csharp
// Get character history (last 30 days)
var history = HistoryTracker.GetCharacterHistory("Character1", 30);
dataGridView.DataSource = history;

// Get daily trends
var trends = HistoryTracker.GetDailyTrends(30);
dataGridView.DataSource = trends;

// Get popular tasks
var popularTasks = HistoryTracker.GetPopularTasks();
dataGridView.DataSource = popularTasks;

// Get character performance
var performance = HistoryTracker.GetCharacterPerformance();
dataGridView.DataSource = performance;
```

**Step 5: Cleanup Old Data (Optional)**

```csharp
// Run cleanup monthly or on demand
// Keeps last 90 days of task history, 180 days of summaries
HistoryTracker.CleanupOldHistory(90);
```

## Complete Integration Example

Here's a complete example showing all features integrated:

```csharp
using AutoVPT.Database;
using AutoVPT.UI;
using System;
using System.Windows.Forms;

namespace AutoVPT
{
    public partial class Form1 : Form
    {
        private StatisticsPanel statisticsPanel;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize database and history tracking
            DatabaseHelper.Initialize();
            HistoryTracker.Initialize();

            // Add statistics panel to form
            InitializeStatisticsPanel();

            // Add menu items for reporting
            AddReportingMenu();

            // Your existing initialization code...
            loadCharacterList();
        }

        private void InitializeStatisticsPanel()
        {
            statisticsPanel = new StatisticsPanel
            {
                Location = new Point(this.Width - 320, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            statisticsPanel.ViewDetailsClicked += (s, e) =>
            {
                ShowDashboard();
            };

            this.Controls.Add(statisticsPanel);
        }

        private void AddReportingMenu()
        {
            // Assuming you have a menu strip
            var menuItemReports = new ToolStripMenuItem("Reports");

            var menuItemDashboard = new ToolStripMenuItem("Dashboard", null, (s, e) => ShowDashboard());
            var menuItemHistory = new ToolStripMenuItem("View History", null, (s, e) => ShowHistory());
            var menuItemTrends = new ToolStripMenuItem("Daily Trends", null, (s, e) => ShowTrends());

            menuItemReports.DropDownItems.Add(menuItemDashboard);
            menuItemReports.DropDownItems.Add(menuItemHistory);
            menuItemReports.DropDownItems.Add(menuItemTrends);

            // Add to main menu (adjust to your menu structure)
            // mainMenuStrip.Items.Add(menuItemReports);
        }

        private void ShowDashboard()
        {
            var dashboard = new ReportingDashboard();
            dashboard.ShowDialog(this);

            // Refresh statistics after dashboard closes
            statisticsPanel?.LoadStatistics();
        }

        private void ShowHistory()
        {
            // Create a simple form to show history
            var historyForm = new Form
            {
                Text = "Task History",
                Size = new System.Drawing.Size(800, 600),
                StartPosition = FormStartPosition.CenterParent
            };

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var trends = HistoryTracker.GetDailyTrends(30);
            dgv.DataSource = trends;

            historyForm.Controls.Add(dgv);
            historyForm.ShowDialog(this);
        }

        private void ShowTrends()
        {
            var trendsForm = new Form
            {
                Text = "Daily Completion Trends (Last 30 Days)",
                Size = new System.Drawing.Size(900, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var trends = HistoryTracker.GetDailyTrends(30);
            dgv.DataSource = trends;

            trendsForm.Controls.Add(dgv);
            trendsForm.ShowDialog(this);
        }

        // Call this when character automation completes
        private void OnCharacterTaskComplete(string characterId, string taskName)
        {
            // Record in history
            HistoryTracker.RecordTaskCompletion(characterId, taskName);

            // Refresh statistics panel
            statisticsPanel?.LoadStatistics();
        }

        // Call this at end of "Auto All" operation
        private void OnAllAutomationComplete()
        {
            // Update daily summary
            HistoryTracker.UpdateDailySummary();

            // Refresh statistics
            statisticsPanel?.LoadStatistics();
        }
    }
}
```

## Task Names Reference

When recording task completions, use these standardized task names:

- `"VipPromotion"` - VIP daily rewards
- `"DoiNangNo"` - Resource exchange
- `"TriAn"` - Gratitude quest
- `"LatTheBai"` - Card flip
- `"RutBo"` - Withdraw
- `"DoiKGDK"` - KGDK exchange
- `"TuHanh"` - Cultivation
- `"TruMa"` - Monster hunting
- `"AoMaThap"` - Demon tower
- `"TrongCay"` - Plant tree
- `"CheMatBao"` - Craft secret treasure
- `"AutoPhuBan"` - Auto dungeon
- `"UocNguyen"` - Make wish
- `"NhanThuongHLVT"` - Receive HLVT reward
- `"NhanHoiPhuc"` - Receive recovery
- `"MeTran"` - Maze battle
- `"HaiThuoc"` - Plant herbs
- `"CauCa"` - Fishing
- `"AutoThanTu"` - Auto temple

## Benefits

### Reporting Dashboard
- Quick overview of all character progress
- Identify characters lagging behind
- Group-based management
- Feature-level statistics

### Statistics Panel
- Always-visible progress monitor
- No need to open separate windows
- Auto-refreshing status
- One-click access to full dashboard

### Historical Tracking
- Track progress over time
- Identify patterns (which tasks completed most)
- Performance analytics per character
- Historical trends for planning

## Tips

1. **Refresh Timing**: Call `statisticsPanel.LoadStatistics()` after:
   - Character task completion
   - "Auto All" finishes
   - Manual refresh button click
   - Character settings changes

2. **History Recording**: Record completions immediately when status changes:
   ```csharp
   // Before
   character.StatusDoiNangNo = 1;

   // After
   character.StatusDoiNangNo = 1;
   HistoryTracker.RecordTaskCompletion(character.ID, "DoiNangNo");
   ```

3. **Performance**: History tracking is lightweight:
   - Uses indexes for fast queries
   - Auto-cleanup of old data
   - Minimal overhead on task completion

4. **Backup**: History is stored in same `mchelper.db` file:
   - Backup the database file to preserve history
   - Export to CSV via DB Browser for SQLite

## Troubleshooting

### Statistics Panel Not Updating

**Problem**: Panel shows old data

**Solution**:
```csharp
statisticsPanel.LoadStatistics(); // Force refresh
```

### Dashboard Shows Empty

**Problem**: No data in dashboard

**Solution**: Ensure `DatabaseHelper.Initialize()` is called before opening dashboard

### History Not Recording

**Problem**: Task completions not appearing in history

**Solution**:
1. Verify `HistoryTracker.Initialize()` was called
2. Check `schema_history.sql` exists in Database folder
3. Verify task name spelling matches reference list

### Database Locked Error

**Problem**: "Database is locked" error

**Solution**: DatabaseHelper and HistoryTracker use internal locking. Ensure you're not accessing database from multiple threads simultaneously.

## Next Steps

1. ✅ Integrate statistics panel into main form
2. ✅ Add dashboard menu item
3. ✅ Record task completions in MainAuto.cs
4. ✅ Set up daily summary updates
5. ✅ Test all features with existing characters
6. ✅ Enjoy enhanced reporting and analytics!

---

**Created:** 2025-12-03
**Status:** Ready for integration
**Requirements:** SQLite migration completed

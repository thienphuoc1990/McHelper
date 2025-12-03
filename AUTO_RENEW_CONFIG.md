# Auto-Renew Configuration Feature

## Problem Solved

Previously, if you clicked "Run Auto" without first clicking on the character row in the table, the automation might fail because the daily status flags weren't reset. You had to manually click on the row first to trigger the configuration renewal.

## Solution

The system now **automatically checks and renews configuration** when you start any automation, so you don't need to click on the character row first.

## How It Works

### Before (Old Behavior)
1. User clicks on character row → triggers `checkRenewConfig()`
2. System checks if date is old
3. If old, sets `renewConfig = true`
4. User then needs to update character → calls `parsingAndUpdateCharacter()`
5. Finally user can click "Run Auto"

**Problem:** If you skip step 1 and go directly to step 5, the automation may fail because status flags from yesterday are still marked as "completed".

### After (New Behavior)
1. User clicks "Run Auto" (or any action button)
2. System **automatically** calls `autoRenewConfigIfNeeded()`
3. System checks if character's date is older than today
4. If old date detected:
   - All status flags are reset to 0 (not completed)
   - Date is updated to today
   - Settings are saved automatically
5. Automation runs with fresh status

**Result:** You can click "Run Auto" anytime without worrying about manually clicking the row first!

## What Gets Auto-Renewed

When a new day is detected, these status flags are automatically reset to 0:

- StatusVipPromotion
- StatusDoiNangNo
- StatusTriAn
- StatusLatTheBai
- StatusRutBo
- StatusDoiKGDK
- StatusTuHanh
- StatusTruMa
- StatusAoMaThap
- StatusTrongCay
- StatusCheMatBao
- StatusAutoPhuBan
- StatusUocNguyen
- StatusNhanThuongHLVT
- StatusNhanHoiPhuc
- StatusMeTran
- StatusHaiThuoc
- StatusCauCa
- StatusAutoThanTu

## Status Messages

You'll see these messages in the status box:

### Normal Operation
- **"Trạng thái đã được cập nhật hôm nay"** - Status already updated today, no renewal needed
- **"Ngày mới, tự động làm mới trạng thái"** - New day detected, automatically renewing status
- **"Ngày không hợp lệ, làm mới cấu hình"** - Invalid date found, renewing config for safety

### Error Handling
- **"Lỗi khi kiểm tra ngày, làm mới cấu hình để an toàn"** - Error checking date, renewing to be safe

## Where It's Applied

Auto-renewal is now enabled in:

1. **Main Auto Button**
   - `buttonRunAuto_Click` - The main "Run Auto" button

2. **Individual Action Buttons** (example)
   - `buttonDaPet_Click` - Pet battle automation
   - (Can be added to other individual buttons as needed)

3. **Batch "All" Operations**
   - `buttonDoiNangNoAll_Click` - Exchange resources for all characters
   - (Auto-renews for EACH character in the batch)

## Code Implementation

### New Methods Added

**`autoRenewConfigIfNeeded()`**
- Checks if character.Date is older than today
- Automatically resets all status flags if old
- Updates date to today
- Saves settings to XML
- Handles errors gracefully with logging

**`resetAllStatusFlags()`**
- Resets all 19 status flags to 0
- Called by `autoRenewConfigIfNeeded()` when renewal is needed

### Usage Example

```csharp
private void buttonRunAuto_Click(object sender, EventArgs e)
{
    if (!checkSelectCharacter()) { return; }

    // Automatically check and renew configuration if needed
    autoRenewConfigIfNeeded();

    // Rest of the automation code...
    IntPtr hWnd = getHandledWindow();
    // ...
}
```

## Benefits

1. ✅ **No More Manual Clicks** - Don't need to click character row before running
2. ✅ **Prevents Failed Runs** - Daily tasks won't skip because status wasn't reset
3. ✅ **Automatic** - Works silently in the background
4. ✅ **Safe** - Has error handling with fallback to renewal
5. ✅ **Logged** - All operations are logged for debugging
6. ✅ **Batch-Friendly** - Works correctly with "All" operations

## Testing Checklist

After building and running:

- [ ] Start automation on a new day without clicking character row
- [ ] Verify status message shows "Ngày mới, tự động làm mới trạng thái"
- [ ] Verify all daily tasks run correctly
- [ ] Check XML file - Date should be updated to today
- [ ] Check XML file - All status flags should be 0
- [ ] Run automation again same day - should see "Trạng thái đã được cập nhật hôm nay"
- [ ] Test with "All" operations - each character should auto-renew
- [ ] Check `/logs/` folder for any errors

## Compatibility

- ✅ **Backwards Compatible** - Old behavior still works (clicking row)
- ✅ **No Breaking Changes** - Existing workflows unchanged
- ✅ **XML Files Compatible** - Works with existing character XML files
- ✅ **Graceful Fallback** - If date parsing fails, renews config for safety

## Error Handling

The system handles these edge cases:

1. **Empty/Null Date** - Assumes new character, sets date to today
2. **Invalid Date Format** - Logs error and renews config
3. **Parse Exception** - Catches exception, logs it, renews config
4. **Save Failure** - Already handled by Helper.saveSettingsToXML()

All errors are logged to `/logs/YYYY-MM-DD.log` for debugging.

## Future Enhancements

Optional improvements you could add:

1. Add auto-renewal to ALL individual action buttons
2. Add UI indicator showing when config was last renewed
3. Add manual "Force Renew" button for testing
4. Add configuration option to disable auto-renewal (for advanced users)
5. Add statistics tracking (how many times renewed per character)

## Troubleshooting

### Automation Still Failing?

**Check these:**
1. Look in logs for errors: `/logs/YYYY-MM-DD.log`
2. Verify character XML has valid date: `database/{CharacterID}.xml`
3. Check if status flags are being reset (should be 0 after renewal)
4. Ensure you rebuilt the project after adding this feature

### Status Not Resetting?

**Possible causes:**
1. `autoRenewConfigIfNeeded()` not being called - check button implementation
2. Date comparison logic issue - check logs
3. XML save failing - check file permissions
4. Character object not being updated in memory

### Want to Force Renewal?

Temporarily set the date in the XML file to an old date (e.g., "01/01/2020") and run automation. It should detect the old date and renew automatically.

---

**This feature makes the application smarter and more user-friendly by eliminating a manual step that users often forgot!** 🎉

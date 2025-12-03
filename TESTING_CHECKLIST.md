# Testing Checklist - Phase 4 Refactored Executors

**Date:** 2025-11-24
**Status:** Ready for Testing
**Executors to Test:** 10 (77% of total)

---

## Testing Categories

### 1. Pure Native Async Executors (4)
### 2. Hybrid Executors (3)
### 3. Already Native Executors (3)

---

## Pure Native Async Executors

### ✅ VipPromotionExecutor
**Feature:** VIP Rewards Collection
**Refactored:** Earlier (Phase 4 template)

**Test Cases:**
- [ ] Opens VIP panel successfully
- [ ] Collects main section rewards (4 attempts)
- [ ] Scrolls down to find additional rewards (15 scrolls max)
- [ ] Collects additional rewards (2 attempts)
- [ ] Closes VIP panel properly
- [ ] Handles missing VIP button gracefully
- [ ] Works with both Chinese and non-Chinese clients

**Expected Behavior:**
- Should collect all available VIP rewards
- Should log each step clearly
- Should return success with rewards count
- Should handle image recognition failures gracefully

**Critical Checks:**
- ✓ No Thread.Abort usage
- ✓ Pure async/await throughout
- ✓ No legacy GeneralFunctions calls
- ✓ Proper error handling

---

### ✅ DoiKGDKExecutor
**Feature:** Space-Time Exchange
**Refactored:** Today (Batch 1)

**Test Cases:**
- [ ] Closes all dialogs before starting
- [ ] Opens quick features list
- [ ] Scrolls to find "khonggiandieukhac" feature
- [ ] Opens space-time carving panel
- [ ] Clicks exchange button
- [ ] Clicks confirm button
- [ ] Closes panels after completion

**Expected Behavior:**
- Should exchange space-time carvings
- Should handle scrolling through quick features
- Should return specific failure messages if panel doesn't open

**Critical Checks:**
- ✓ No Task.Run wrapping
- ✓ Reusable OpenFeatureFromQuickListAsync helper
- ✓ Better error messages than legacy
- ✓ No legacy dependencies

**Known Helper Methods:**
- `OpenFeatureFromQuickListAsync()` - Reusable for other features

---

### ✅ RutBoExecutor
**Feature:** Equipment Withdrawal
**Refactored:** Today (Batch 1)

**Test Cases:**
- [ ] Closes all dialogs
- [ ] Opens character panel
- [ ] Opens wardrobe (tudo)
- [ ] Clicks all withdraw buttons (clickAll behavior)
- [ ] Clicks withdraw reward button
- [ ] Clicks confirm button
- [ ] Closes panels after completion

**Expected Behavior:**
- Should withdraw all available equipment
- Should handle multiple withdraw buttons
- Should retry finding images with loop

**Critical Checks:**
- ✓ No Task.Run wrapping
- ✓ ClickImageWithLoopAsync helper
- ✓ ClickAllImagesWithLoopAsync helper
- ✓ Clear step-by-step logging

**Known Helper Methods:**
- `ClickImageWithLoopAsync()` - Retry until found
- `ClickAllImagesWithLoopAsync()` - Click all instances

---

### ✅ NhanHoiPhucExecutor
**Feature:** Recovery Rewards
**Refactored:** Today (Batch 1)

**Test Cases:**
- [ ] Closes all dialogs
- [ ] Opens daily quest panel
- [ ] Opens recovery panel
- [ ] Collects profession quest recovery
- [ ] Collects pet training recovery
- [ ] Collects gratitude quest recovery
- [ ] Collects monster hunting recovery
- [ ] Collects bounty recovery
- [ ] Returns count of collected rewards

**Expected Behavior:**
- Should collect all 5 recovery reward types
- Should handle missing rewards gracefully
- Should return success message with count

**Critical Checks:**
- ✓ No Task.Run wrapping
- ✓ WaitForPanelAsync helper
- ✓ CollectRecoveryRewardAsync with offset
- ✓ Counts rewards collected

**Known Helper Methods:**
- `WaitForPanelAsync()` - Wait for panel with retries
- `CollectRecoveryRewardAsync()` - Click with Point offset

---

## Hybrid Executors (Use AutoFeatures for Navigation)

### ⚠️ NhanThuongHLVTExecutor
**Feature:** Corridor Rewards
**Refactored:** Today (Batch 2)

**Test Cases:**
- [ ] Closes all dialogs
- [ ] Navigates to Quyền Cô Thành map
- [ ] Flies up
- [ ] Moves to corridor NPC
- [ ] Flies down
- [ ] Talks to NPC (with -40 Y offset)
- [ ] Scrolls down in dialog (3 times)
- [ ] Clicks receive rewards button

**Expected Behavior:**
- Should navigate to map successfully
- Should find and talk to NPC
- Should collect corridor rewards

**Critical Checks:**
- ✓ Removed GeneralFunctions dependency
- ✓ Clear step-by-step logging
- ✓ Better error messages
- ⚠️ Still uses AutoFeatures for navigation

**Navigation Methods Used:**
- moveToMap, moveToNPC, bay, bayXuong, talkToNPC

---

### ⚠️ TuHanhExecutor
**Feature:** Cultivation Quest
**Refactored:** Today (Batch 2)

**Test Cases:**
- [ ] Closes all dialogs
- [ ] Flies up
- [ ] Uses NVHN quest helper to navigate
- [ ] Reaches cultivation NPC (truonglaovouutoc)
- [ ] Opens auto cultivation dialog
- [ ] Clicks start cultivation button
- [ ] Confirms cultivation start
- [ ] Waits for cultivation to begin

**Expected Behavior:**
- Should use NVHN to find NPC automatically
- Should start cultivation successfully
- Should verify cultivation started (check image)

**Critical Checks:**
- ✓ Removed GeneralFunctions dependency
- ✓ Extracted StartAutoCultivation helper
- ⚠️ Still uses AutoFeatures for NVHN navigation

**Navigation Methods Used:**
- bay, openQuestByNVHN, isTalkWithNPC

---

### ⚠️ AutoThanTuExecutor
**Feature:** Divine Cultivation
**Refactored:** Today (Batch 2)

**Test Cases:**
- [ ] Closes all dialogs
- [ ] Navigates to Quyền Cô Thành map (7, -18 offset)
- [ ] Flies up
- [ ] Moves to divine cultivation NPC
- [ ] Flies down
- [ ] Talks to NPC
- [ ] Clicks divine cultivation option
- [ ] Clicks start button
- [ ] Confirms start

**Expected Behavior:**
- Should navigate to map successfully
- Should find and talk to NPC
- Should start divine cultivation

**Critical Checks:**
- ✓ Removed GeneralFunctions dependency
- ✓ Extracted StartDivineCultivation helper
- ⚠️ Still uses AutoFeatures for navigation

**Navigation Methods Used:**
- moveToMap, moveToNPC, bay, bayXuong, talkToNPC

---

## Already Native Executors

### ✅ CheMatBaoExecutor
**Feature:** Secret Manual Crafting
**Status:** Already Native (No changes needed)

**Test Cases:**
- [ ] Opens character panel
- [ ] Opens soul panel
- [ ] Opens secret manual panel
- [ ] Opens crafting tab
- [ ] Selects manual tier (1-10)
- [ ] Selects manual type (Thần Binh, Pháp Sức, etc.)
- [ ] Auto-places materials
- [ ] Crafts manuals until out of attempts
- [ ] Returns count of crafted manuals

**Expected Behavior:**
- Should craft specified manual type and tier
- Should stop when out of crafting attempts
- Should handle "out of attempts" image

**Critical Checks:**
- ✓ Already pure async/await
- ✓ Has ClickImageByGroupAsync helper
- ✓ Has FindImageByGroupAsync helper
- ✓ Proper tier selection with offset calculation

---

### ✅ TrongNLExecutor
**Feature:** Material Planting
**Status:** Already Native (No changes needed)

**Test Cases:**
- [ ] Opens farm interface
- [ ] Opens farming panel
- [ ] Checks for empty plots
- [ ] Selects material type
- [ ] Plants on all empty plots (with -25 Y offset)
- [ ] Harvests mature materials
- [ ] Returns count of planted plots

**Expected Behavior:**
- Should plant specified material type
- Should plant on all empty plots
- Should harvest ready materials

**Critical Checks:**
- ✓ Already pure async/await
- ✓ Clear step-by-step structure
- ✓ GetMaterialImagePath helper for 10 material types

---

### ✅ TriAnExecutor
**Feature:** Gratitude Quest
**Status:** Already Native (No changes needed)

**Test Cases:**
- [ ] Checks if quest already completed
- [ ] Accepts quest from NPC
- [ ] Navigates to quest area
- [ ] Defeats required monsters
- [ ] Turns in quest for rewards
- [ ] Handles different VIP levels (0-10)

**Expected Behavior:**
- Should complete gratitude quest
- Should handle VIP tier differences
- Should track monster kills

**Critical Checks:**
- ✓ Already pure async/await
- ✓ VIP level support in constructor
- ✓ Monster target tracking
- ✓ Complex quest flow handling

---

## Integration Testing

### Test Scenarios

#### Scenario 1: Run All Features
**Steps:**
1. Enable all 10 refactored features in character config
2. Click "Run Auto All" button
3. Observe execution order and completion

**Expected:**
- All features execute in sequence
- Each feature logs clearly
- No crashes or hangs
- Character.Running flag respected
- Status updates visible in UI

#### Scenario 2: Stop All During Execution
**Steps:**
1. Start "Run Auto All" with multiple features
2. Wait for 2-3 features to start
3. Click "Stop All" button

**Expected:**
- All executors stop gracefully
- No crashes (Thread.Abort removed)
- Status shows stopped state
- No zombie threads remain

#### Scenario 3: Individual Feature Buttons
**Steps:**
1. Test each feature's individual button
2. Verify feature executes correctly
3. Check status updates

**Expected:**
- Single feature executes
- Status tracking works
- XML persistence works (status saved)

#### Scenario 4: Error Handling
**Steps:**
1. Test with game window not focused
2. Test with missing images
3. Test with network issues

**Expected:**
- Graceful error messages
- No crashes
- Clear failure reasons in logs

---

## Performance Testing

### Metrics to Track

**For Each Executor:**
- [ ] Execution time (from start to finish)
- [ ] Memory usage (no leaks)
- [ ] Image recognition success rate
- [ ] Number of retries needed
- [ ] CPU usage during execution

**Baseline Expectations:**
- VipPromotion: 30-60 seconds
- DoiKGDK: 20-40 seconds
- RutBo: 15-30 seconds
- NhanHoiPhuc: 30-50 seconds
- NhanThuongHLVT: 60-90 seconds (navigation)
- TuHanh: 30-60 seconds (navigation)
- AutoThanTu: 60-90 seconds (navigation)
- CheMatBao: 2-5 minutes (crafting loop)
- TrongNL: 1-2 minutes (planting)
- TriAn: 5-10 minutes (quest completion)

---

## Logging & Debugging

### Check Log Quality

**For Each Executor:**
- [ ] Logs feature start
- [ ] Logs each major step
- [ ] Logs success/failure clearly
- [ ] Logs useful context (counts, types, etc.)
- [ ] Logs errors with stack traces

**Log Format:**
```
[HH:MM:SS] [INFO/WARN/ERROR] [CharacterID] [FeatureName] Message
```

**Example Good Logs:**
```
[10:30:45] [INFO] [char123] [VipPromotion] Starting VIP Rewards feature
[10:30:46] [INFO] [char123] [VipPromotion] Opening VIP panel...
[10:30:48] [INFO] [char123] [VipPromotion] Collecting VIP rewards (main section)...
[10:30:52] [INFO] [char123] [VipPromotion] Scrolling down for additional rewards...
[10:30:58] [INFO] [char123] [VipPromotion] VIP Rewards completed successfully
```

---

## Known Issues & Workarounds

### Pure Native Executors
**DoiKGDKExecutor:**
- May fail if quick features list is in unusual state
- Workaround: Restart game if scroll fails

**RutBoExecutor:**
- Multiple withdraw buttons may not all be found
- Workaround: Run feature twice if needed

**NhanHoiPhucExecutor:**
- Some recovery types may not be available
- Workaround: Expected behavior, not an error

### Hybrid Executors
**NhanThuongHLVTExecutor:**
- Navigation can fail if map not unlocked
- Workaround: Check character level requirements

**TuHanhExecutor:**
- NVHN navigation may timeout
- Workaround: Ensure quest is available for character level

**AutoThanTuExecutor:**
- Map navigation can fail
- Workaround: Ensure map is unlocked

### Already Native Executors
**CheMatBaoExecutor:**
- Complex UI navigation may fail
- Known issue: Wait times may need adjustment

**TrongNLExecutor:**
- Farm button may not be found
- Known issue: Expected if farm UI state unusual

**TriAnExecutor:**
- VIP tier differences in quest objectives
- Known issue: Configured per character VIP level

---

## Success Criteria

### Must Have ✅
- [ ] All 10 executors build without errors
- [ ] All 10 executors execute without crashes
- [ ] Stop All button works with all executors
- [ ] No Thread.Abort usage in refactored code
- [ ] Clear logging for all executors

### Should Have ⚠️
- [ ] All pure native executors have no legacy dependencies
- [ ] Hybrid executors document AutoFeatures usage
- [ ] Performance within expected ranges
- [ ] Error messages are helpful

### Nice to Have 💡
- [ ] Unit tests for pure native executors
- [ ] Benchmark comparisons vs legacy
- [ ] Documentation of helper methods
- [ ] Migration guide for remaining features

---

## Sign-Off

### Developer Checklist
- [ ] Code reviewed
- [ ] Build successful
- [ ] Manual testing completed
- [ ] Known issues documented
- [ ] Ready for production

### Testing Checklist
- [ ] All test cases executed
- [ ] Integration tests passed
- [ ] Performance acceptable
- [ ] Error handling verified

### Documentation Checklist
- [ ] Patterns documented
- [ ] Helper methods documented
- [ ] Known issues documented
- [ ] Migration notes updated

---

## Next Steps After Testing

**If All Tests Pass:**
1. Commit changes with detailed message
2. Create PHASE4_COMPLETE.md summary
3. Plan Phase 5 (navigation service or other improvements)

**If Issues Found:**
1. Document all issues
2. Prioritize critical vs nice-to-have fixes
3. Fix critical issues
4. Re-test
5. Document known issues for non-critical items

---

## Quick Test Script

```bash
# Build project
cd /mnt/c/Users/ADMIN/source/repos/McHelper/v1
"/mnt/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" AutoVPT.sln /p:Configuration=Debug /p:Platform=x86

# Run application
./bin/x86/Debug/VPT_Supporter.exe

# Test each feature manually:
# 1. Configure character with all 10 features enabled
# 2. Run "Auto All" - verify all execute
# 3. Test "Stop All" - verify graceful shutdown
# 4. Check logs for clarity and completeness
# 5. Verify XML status persistence
```

---

## Testing Log Template

```markdown
## Test Session: [Date]
**Tester:** [Name]
**Build:** [Commit Hash]
**Environment:** [Chinese/Non-Chinese Client]

### VipPromotionExecutor
- Status: [ ] Pass [ ] Fail [ ] Not Tested
- Issues:
- Notes:

### DoiKGDKExecutor
- Status: [ ] Pass [ ] Fail [ ] Not Tested
- Issues:
- Notes:

[... repeat for all 10 executors ...]

### Integration Tests
- Run All: [ ] Pass [ ] Fail
- Stop All: [ ] Pass [ ] Fail
- Individual Buttons: [ ] Pass [ ] Fail

### Performance
- All features within expected times: [ ] Yes [ ] No
- Memory leaks detected: [ ] Yes [ ] No
- CPU usage acceptable: [ ] Yes [ ] No

### Overall Assessment
- Ready for production: [ ] Yes [ ] No [ ] With Notes
- Critical issues: [List]
- Recommended actions: [List]
```

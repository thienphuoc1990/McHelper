# XML to SQLite Cleanup Guide

## Overview

After successful migration to SQLite and confirming everything works, you can safely remove old XML-related code and files.

## Files That Can Be Removed

### 1. DML/XMLCharacter.cs
This file is **no longer used**. All its functionality has been replaced by `Database/DatabaseHelper.cs`.

**To remove:**
1. Delete `v1/DML/XMLCharacter.cs`
2. If `DML` folder is now empty, delete the entire `DML/` folder

### 2. XML Data Files (After Backup)

Once you've confirmed SQLite is working correctly:

**Backup first:**
```bash
# These should already exist from migration
database_backup_{timestamp}/
```

**Then remove:**
- `database/*.xml` - Individual character XML files
- `database/data.xml` - Character list XML file

**Keep:**
- `database/mchelper.db` - Your new SQLite database
- `database_backup_*/` - XML backups (for safety)

## Code Already Updated

✅ **Helper.cs**
- `saveSettingsToXML()` - Now uses `DatabaseHelper.SaveCharacter()`
- `loadSettingsFromXML()` - Now uses `DatabaseHelper.LoadCharacter()`
- Removed `System.Xml.Serialization` using statement

✅ **Character.cs / CharacterList class**
- `GetCharacter()` - Now uses `DatabaseHelper.LoadCharacter()`
- `GetCharacterList()` - Now uses `DatabaseHelper.LoadAllCharacters()`
- `InsertCharacter()` - Now uses `DatabaseHelper.SaveCharacter()`
- `UpdateCharacter()` - Now uses `DatabaseHelper.SaveCharacter()`
- `DeleteCharacter()` - Now uses `DatabaseHelper.DeleteCharacter()`
- Removed `AutoVPT.DML` and `System.Xml.Serialization` using statements

## Cleanup Steps

### Step 1: Verify SQLite is Working

Test all operations:
- [ ] Load characters
- [ ] Save characters
- [ ] Delete characters
- [ ] Auto All operations
- [ ] Daily status reset
- [ ] Group operations

### Step 2: Keep Backups for Safety Period

Recommended: Keep XML backups for **2 weeks** while monitoring production usage.

### Step 3: Remove Old XML Files

After safety period and confirmation everything works:

```bash
# Windows Command Prompt
cd database
del *.xml

# Or keep as archive
mkdir archive_xml
move *.xml archive_xml\
```

### Step 4: Remove XMLCharacter.cs

In Visual Studio:
1. Right-click `DML/XMLCharacter.cs` → Delete
2. Confirm deletion
3. If `DML` folder is empty, delete it too

### Step 5: Update Project References

If you used any direct references to `XMLCharacter`:

**Before:**
```csharp
using AutoVPT.DML;
...
XMLCharacter.Insert(id, link, group);
```

**After:**
```csharp
using AutoVPT.Database;
...
DatabaseHelper.SaveCharacter(character);
```

But this should already be handled by the `CharacterList` class updates.

## Database Initialization

### Add to Application Startup

In your main form's `Load` event or application startup:

```csharp
using AutoVPT.Database;

private void Form1_Load(object sender, EventArgs e)
{
    // Initialize SQLite database (creates tables if not exist)
    DatabaseHelper.Initialize();

    // Your existing code...
    loadCharacterList();
}
```

## Rollback Plan (If Needed)

If you need to rollback to XML after cleanup:

1. Restore XML files from `database_backup_*/` folder
2. Revert `Helper.cs` changes (use git)
3. Revert `Character.cs` changes (use git)
4. Restore `XMLCharacter.cs` (use git)

## What to Keep

✅ **Keep these files:**
- `Database/DatabaseHelper.cs` - SQLite operations
- `Database/MigrationUtility.cs` - Migration tools (for future reference)
- `Database/schema.sql` - Database schema
- `Database/MIGRATION_GUIDE.md` - Documentation
- `Database/README.md` - Documentation
- `database/mchelper.db` - Your database
- `database_backup_*/` - XML backups (for safety period)

❌ **Can be removed after verification:**
- `DML/XMLCharacter.cs` - Replaced by DatabaseHelper
- `database/*.xml` - Individual character files (after backup)
- `database/data.xml` - Character list file (after backup)

🤔 **Optional (can remove later):**
- `Database/TestMigration.cs` - Test form (not needed in production)
- `Database/MigrationUtility.cs` - After successful migration

## Size Comparison

After cleanup, you'll have:

**Before (50 characters):**
- 51 XML files (50 characters + 1 data.xml) ≈ 500 KB
- Slow bulk operations

**After (50 characters):**
- 1 SQLite database file ≈ 200 KB
- Fast bulk operations
- Queryable data

## Checklist

- [ ] SQLite migration completed successfully
- [ ] All features tested and working
- [ ] XML backups created and verified
- [ ] Safety period completed (recommended: 2 weeks)
- [ ] `DatabaseHelper.Initialize()` added to application startup
- [ ] Old XML files removed (or archived)
- [ ] `XMLCharacter.cs` removed from project
- [ ] Empty `DML/` folder removed
- [ ] Git commit with cleanup changes

## Support

If you encounter any issues after cleanup:
1. Restore from XML backups in `database_backup_*/`
2. Use git to revert code changes
3. Check application logs for errors
4. Verify `mchelper.db` file is not corrupted

---

**Status:** Ready for cleanup after verification period
**Last Updated:** 2025-12-03

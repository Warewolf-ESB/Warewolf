# Azure Functions File System Setup Guide

This guide explains how to set up the required folders and files in Azure Functions (via Kudu) to test Warewolf file and folder tools.

## Why Is This Necessary?

Azure Functions run in a **sandboxed environment** with restricted file system access. Unlike your local development machine where you can access any path like `C:\Temp\` or `D:\MyFolder\`, Azure Functions can only access specific directories.

### Paths That DO NOT Work on Azure

| Path | Reason |
|------|--------|
| `C:\Temp\` | System drive - Access denied |
| `C:\Users\` | System drive - Access denied |
| `D:\MyFolder\` | Root D: drive - Access denied |
| `D:\Learning\` | Not under allowed paths - Access denied |
| `\\server\share\` | UNC paths - Not available |

### Paths That WORK on Azure

| Path | Type | Persistent | Use Case |
|------|------|------------|----------|
| `D:\home\data\` | Azure Files Mount | Yes | **Recommended for data** |
| `D:\home\site\wwwroot\` | Azure Files Mount | Yes | Deployed code (read-only recommended) |
| `D:\home\LogFiles\` | Azure Files Mount | Yes | Application logs |
| `D:\local\Temp\` | Local SSD | No | Temporary files only |

---

## File and Folder Tools - Required Setup

### Overview of All Tools

| Tool | Source Folder Needed | Destination Folder Needed | Files Needed |
|------|---------------------|---------------------------|--------------|
| **Copy** | Yes | Yes | Yes |
| **Create** | No | Yes | No |
| **Delete** | Yes | No | Yes |
| **Move** | Yes | Yes | Yes |
| **Read File** | Yes | No | Yes |
| **Read Folder** | Yes | No | Yes (for listing) |
| **Rename** | Yes | No | Yes |
| **Zip** | Yes | Yes | Yes |
| **Unzip** | Yes | Yes | Yes (.zip file) |
| **Write** | No | Yes | No |

---

## Complete Setup Script

Run this script in **Kudu PowerShell Console** (`https://<your-app>.scm.azurewebsites.net/DebugConsole/?shell=powershell`):

```powershell
# ============================================================
# WAREWOLF FILE & FOLDER TOOLS - AZURE SETUP SCRIPT
# ============================================================
# Run this script in Kudu PowerShell to create all required
# folders and test files for Warewolf file/folder tools.
# ============================================================

$baseDir = "D:\home\data\Warewolf"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Warewolf Azure File System Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# ------------------------------------------------------------
# STEP 1: Create Base Directory Structure
# ------------------------------------------------------------
Write-Host "`n[1/5] Creating directory structure..." -ForegroundColor Yellow

$folders = @(
    "$baseDir",
    "$baseDir\Source",
    "$baseDir\Source\SubFolder1",
    "$baseDir\Source\SubFolder2",
    "$baseDir\Destination",
    "$baseDir\CopyDestination",
    "$baseDir\MoveDestination",
    "$baseDir\ZipOutput",
    "$baseDir\UnzipOutput",
    "$baseDir\CreateFolder",
    "$baseDir\WriteFolder",
    "$baseDir\DeleteFolder",
    "$baseDir\RenameFolder",
    "$baseDir\ReadFolder",
    "$baseDir\ReadFolder\Level1",
    "$baseDir\ReadFolder\Level1\Level2"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
    Write-Host "  Created: $folder" -ForegroundColor Green
}

# ------------------------------------------------------------
# STEP 2: Create Test Files for Read/Copy/Move/Delete
# ------------------------------------------------------------
Write-Host "`n[2/5] Creating test files..." -ForegroundColor Yellow

# Source folder files
"This is test file 1 content" | Out-File -FilePath "$baseDir\Source\TestFile1.txt" -Encoding UTF8
"This is test file 2 content" | Out-File -FilePath "$baseDir\Source\TestFile2.txt" -Encoding UTF8
"Sample data for processing" | Out-File -FilePath "$baseDir\Source\SampleData.txt" -Encoding UTF8
"Configuration settings here" | Out-File -FilePath "$baseDir\Source\Config.json" -Encoding UTF8

# Subfolder files
"Subfolder 1 file content" | Out-File -FilePath "$baseDir\Source\SubFolder1\SubFile1.txt" -Encoding UTF8
"Subfolder 2 file content" | Out-File -FilePath "$baseDir\Source\SubFolder2\SubFile2.txt" -Encoding UTF8

# Files for Delete testing
"Delete me 1" | Out-File -FilePath "$baseDir\DeleteFolder\ToDelete1.txt" -Encoding UTF8
"Delete me 2" | Out-File -FilePath "$baseDir\DeleteFolder\ToDelete2.txt" -Encoding UTF8
"Delete me 3" | Out-File -FilePath "$baseDir\DeleteFolder\ToDelete3.txt" -Encoding UTF8

# Files for Rename testing
"Rename this file" | Out-File -FilePath "$baseDir\RenameFolder\OldName.txt" -Encoding UTF8
"Another rename file" | Out-File -FilePath "$baseDir\RenameFolder\BeforeRename.txt" -Encoding UTF8

# Files for ReadFolder testing (multiple levels)
"Level 0 file" | Out-File -FilePath "$baseDir\ReadFolder\RootFile.txt" -Encoding UTF8
"Level 1 file A" | Out-File -FilePath "$baseDir\ReadFolder\Level1\FileA.txt" -Encoding UTF8
"Level 1 file B" | Out-File -FilePath "$baseDir\ReadFolder\Level1\FileB.txt" -Encoding UTF8
"Level 2 file" | Out-File -FilePath "$baseDir\ReadFolder\Level1\Level2\DeepFile.txt" -Encoding UTF8

Write-Host "  Created all test files" -ForegroundColor Green

# ------------------------------------------------------------
# STEP 3: Create Test Files for ReadFile Tool
# ------------------------------------------------------------
Write-Host "`n[3/5] Creating ReadFile test content..." -ForegroundColor Yellow

# Create a multi-line file for ReadFile testing
$multiLineContent = @"
Line 1: This is the first line
Line 2: This is the second line
Line 3: Hello from Warewolf!
Line 4: Testing ReadFile tool
Line 5: Azure Functions compatible
Line 6: D:\home\data is the way
Line 7: End of test file
"@
$multiLineContent | Out-File -FilePath "$baseDir\Source\MultiLineFile.txt" -Encoding UTF8

# JSON file for ReadFile
$jsonContent = @"
{
    "name": "Warewolf Test",
    "version": "1.0.0",
    "azure": true,
    "paths": {
        "source": "D:\\home\\data\\Warewolf\\Source",
        "destination": "D:\\home\\data\\Warewolf\\Destination"
    }
}
"@
$jsonContent | Out-File -FilePath "$baseDir\Source\TestData.json" -Encoding UTF8

# XML file for ReadFile
$xmlContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<workflow>
    <name>Test Workflow</name>
    <version>1.0</version>
    <steps>
        <step id="1">Read File</step>
        <step id="2">Process Data</step>
        <step id="3">Write Output</step>
    </steps>
</workflow>
"@
$xmlContent | Out-File -FilePath "$baseDir\Source\TestData.xml" -Encoding UTF8

Write-Host "  Created ReadFile test content" -ForegroundColor Green

# ------------------------------------------------------------
# STEP 4: Create ZIP File for Unzip Testing
# ------------------------------------------------------------
Write-Host "`n[4/5] Creating ZIP file for Unzip testing..." -ForegroundColor Yellow

# Create temp files for zipping
$zipSourceDir = "$baseDir\TempZipSource"
New-Item -ItemType Directory -Path $zipSourceDir -Force | Out-Null
"Zip content 1" | Out-File -FilePath "$zipSourceDir\ZipFile1.txt" -Encoding UTF8
"Zip content 2" | Out-File -FilePath "$zipSourceDir\ZipFile2.txt" -Encoding UTF8
"Zip content 3" | Out-File -FilePath "$zipSourceDir\ZipFile3.txt" -Encoding UTF8

# Create the ZIP file
$zipPath = "$baseDir\Source\TestArchive.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$zipSourceDir\*" -DestinationPath $zipPath -Force

# Cleanup temp directory
Remove-Item -Path $zipSourceDir -Recurse -Force

Write-Host "  Created: $zipPath" -ForegroundColor Green

# ------------------------------------------------------------
# STEP 5: Verify Setup
# ------------------------------------------------------------
Write-Host "`n[5/5] Verifying setup..." -ForegroundColor Yellow

Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host "  SETUP COMPLETE - Verification" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

Write-Host "`nFolder Structure:" -ForegroundColor White
Get-ChildItem -Path $baseDir -Recurse -Directory | ForEach-Object {
    $indent = "  " * ($_.FullName.Split('\').Count - $baseDir.Split('\').Count)
    Write-Host "$indent$($_.Name)" -ForegroundColor Blue
}

Write-Host "`nFiles Created:" -ForegroundColor White
$files = Get-ChildItem -Path $baseDir -Recurse -File
$files | ForEach-Object {
    $relativePath = $_.FullName.Replace($baseDir, "")
    Write-Host "  $relativePath ($($_.Length) bytes)" -ForegroundColor Gray
}

Write-Host "`nTotal: $($files.Count) files created" -ForegroundColor Green
Write-Host "Ready for Warewolf file/folder tool testing!" -ForegroundColor Green
```

---

## Tool-Specific Workflow Configurations

### 1. Copy Tool

| Field | Value |
|-------|-------|
| File or Folder (Source) | `D:\home\data\Warewolf\Source\TestFile1.txt` |
| Destination | `D:\home\data\Warewolf\CopyDestination\TestFile1.txt` |

**To copy entire folder:**

| Field | Value |
|-------|-------|
| File or Folder (Source) | `D:\home\data\Warewolf\Source\` |
| Destination | `D:\home\data\Warewolf\CopyDestination\` |

---

### 2. Create Tool

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\CreateFolder\NewFile.txt` |

**To create folder:**

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\CreateFolder\NewSubFolder` |

---

### 3. Delete Tool

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\DeleteFolder\ToDelete1.txt` |

**To delete folder:**

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\DeleteFolder\` |

---

### 4. Move Tool

| Field | Value |
|-------|-------|
| File or Folder (Source) | `D:\home\data\Warewolf\Source\SampleData.txt` |
| Destination | `D:\home\data\Warewolf\MoveDestination\SampleData.txt` |

> **Note:** Move deletes the source file after copying. Re-run the setup script to restore test files.

---

### 5. Read File Tool

| Field | Value |
|-------|-------|
| File Path | `D:\home\data\Warewolf\Source\TestFile1.txt` |

**For multi-line file:**

| Field | Value |
|-------|-------|
| File Path | `D:\home\data\Warewolf\Source\MultiLineFile.txt` |

**For JSON file:**

| Field | Value |
|-------|-------|
| File Path | `D:\home\data\Warewolf\Source\TestData.json` |

---

### 6. Read Folder Tool

| Field | Value |
|-------|-------|
| Directory | `D:\home\data\Warewolf\ReadFolder\` |
| Read | Files / Folders / Files & Folders |

**For nested folders:**

| Field | Value |
|-------|-------|
| Directory | `D:\home\data\Warewolf\ReadFolder\Level1\` |

---

### 7. Rename Tool

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\RenameFolder\OldName.txt` |
| New Name | `D:\home\data\Warewolf\RenameFolder\NewName.txt` |

---

### 8. Write Tool

| Field | Value |
|-------|-------|
| File Path | `D:\home\data\Warewolf\WriteFolder\OutputFile.txt` |
| Contents | `Hello from Warewolf!` |

**Append mode:**

| Field | Value |
|-------|-------|
| File Path | `D:\home\data\Warewolf\WriteFolder\OutputFile.txt` |
| Append | True |

---

### 9. Zip Tool

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\Source\` |
| Destination (ZIP) | `D:\home\data\Warewolf\ZipOutput\Archive.zip` |

**Zip single file:**

| Field | Value |
|-------|-------|
| File or Folder | `D:\home\data\Warewolf\Source\TestFile1.txt` |
| Destination | `D:\home\data\Warewolf\ZipOutput\SingleFile.zip` |

---

### 10. Unzip Tool

| Field | Value |
|-------|-------|
| ZIP File | `D:\home\data\Warewolf\Source\TestArchive.zip` |
| Destination | `D:\home\data\Warewolf\UnzipOutput\` |

---

## Folder Structure Diagram

After running the setup script, your Azure Functions file system will have:

```
D:\home\data\Warewolf\
│
├── Source\                             <- Main source folder
│   ├── TestFile1.txt
│   ├── TestFile2.txt
│   ├── SampleData.txt
│   ├── Config.json
│   ├── MultiLineFile.txt
│   ├── TestData.json
│   ├── TestData.xml
│   ├── TestArchive.zip                 <- For Unzip testing
│   ├── SubFolder1\
│   │   └── SubFile1.txt
│   └── SubFolder2\
│       └── SubFile2.txt
│
├── Destination\                        <- General destination
├── CopyDestination\                    <- Copy tool output
├── MoveDestination\                    <- Move tool output
├── ZipOutput\                          <- Zip tool output
├── UnzipOutput\                        <- Unzip tool output
├── CreateFolder\                       <- Create tool output
├── WriteFolder\                        <- Write tool output
│
├── DeleteFolder\                       <- Files to delete
│   ├── ToDelete1.txt
│   ├── ToDelete2.txt
│   └── ToDelete3.txt
│
├── RenameFolder\                       <- Files to rename
│   ├── OldName.txt
│   └── BeforeRename.txt
│
└── ReadFolder\                         <- Folder structure for reading
    ├── RootFile.txt
    └── Level1\
        ├── FileA.txt
        ├── FileB.txt
        └── Level2\
            └── DeepFile.txt
```

---

## Cleanup Script

To reset the test environment (remove all files and start fresh):

```powershell
# Run in Kudu PowerShell

$baseDir = "D:\home\data\Warewolf"

Write-Host "Cleaning up Warewolf test folders..." -ForegroundColor Yellow

if (Test-Path $baseDir) {
    Remove-Item -Path $baseDir -Recurse -Force
    Write-Host "Removed: $baseDir" -ForegroundColor Green
} else {
    Write-Host "Folder not found: $baseDir" -ForegroundColor Yellow
}

Write-Host "`nRun the setup script again to recreate test files." -ForegroundColor Cyan
```

---

## Verification Commands

Run these in Kudu to verify your setup:

```powershell
# Quick verification
$baseDir = "D:\home\data\Warewolf"

Write-Host "=== Source Files ===" -ForegroundColor Cyan
Get-ChildItem "$baseDir\Source" -Recurse | Format-Table Name, Length, LastWriteTime

Write-Host "`n=== All Folders ===" -ForegroundColor Cyan
Get-ChildItem $baseDir -Directory | Format-Table Name

Write-Host "`n=== Total File Count ===" -ForegroundColor Cyan
$count = (Get-ChildItem $baseDir -Recurse -File).Count
Write-Host "Total files: $count" -ForegroundColor Green
```

---

## Quick Reference Card

| Tool | Source Path | Destination Path |
|------|-------------|------------------|
| **Copy** | `D:\home\data\Warewolf\Source\` | `D:\home\data\Warewolf\CopyDestination\` |
| **Create** | N/A | `D:\home\data\Warewolf\CreateFolder\` |
| **Delete** | `D:\home\data\Warewolf\DeleteFolder\` | N/A |
| **Move** | `D:\home\data\Warewolf\Source\` | `D:\home\data\Warewolf\MoveDestination\` |
| **Read File** | `D:\home\data\Warewolf\Source\TestFile1.txt` | N/A |
| **Read Folder** | `D:\home\data\Warewolf\ReadFolder\` | N/A |
| **Rename** | `D:\home\data\Warewolf\RenameFolder\OldName.txt` | N/A |
| **Write** | N/A | `D:\home\data\Warewolf\WriteFolder\` |
| **Zip** | `D:\home\data\Warewolf\Source\` | `D:\home\data\Warewolf\ZipOutput\` |
| **Unzip** | `D:\home\data\Warewolf\Source\TestArchive.zip` | `D:\home\data\Warewolf\UnzipOutput\` |

---

## Troubleshooting

### Error: "Access to the path is denied"

| Cause | Solution |
|-------|----------|
| Using `C:\` or root `D:\` | Use `D:\home\data\` paths |
| Folder doesn't exist | Run the setup script |
| Using wrong path format | Use single `\` not `\\` |

### Error: "Could not find file"

| Cause | Solution |
|-------|----------|
| File not created | Run setup script |
| Typo in path | Check exact path spelling |
| File was moved/deleted | Re-run setup script |

### Error: "Directory not found"

| Cause | Solution |
|-------|----------|
| Parent folder missing | Create parent folder first |
| Path doesn't exist | Verify path in Kudu file browser |

---

## Related Documentation

- [Azure Functions File System](https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference#file-system-access)
- [Kudu Console Documentation](https://github.com/projectkudu/kudu/wiki/Kudu-console)


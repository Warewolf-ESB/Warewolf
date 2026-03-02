# X6 Conversion Template Usage Guide

## Table of Contents
1. [Quick Start](#quick-start)
2. [Detailed Walkthrough](#detailed-walkthrough)
3. [Usage Methods](#usage-methods)
4. [Real Examples](#real-examples)
5. [Troubleshooting](#troubleshooting)

---

## Quick Start

### Prerequisites
- Activity class already exists in the codebase
- Activity inherits from `DsfAbstractFileActivity` or similar base class
- You know all properties that need to be serialized

### 5-Minute Setup

1. **Open the template:** `Docs/X6ConversionTemplate.md`
2. **Fill in the Activity Configuration section** at the top
3. **List all properties** in the properties table
4. **Follow Steps 1-9** replacing tokens with your values
5. **Build and test**

---

## Detailed Walkthrough

### Phase 1: Preparation (5 minutes)

#### 1.1 Gather Information

Open your activity class file and identify:

**Activity Information:**
```csharp
// From the class declaration
[ToolDescriptorInfo("FileFolder-Write", "Write File", ...)]
public class DsfFileWrite : DsfAbstractFileActivity
```
- **ActivityClassName**: `DsfFileWrite`
- **DisplayName**: `"Write File"` (from ToolDescriptorInfo)
- **ActivityFilePath**: Right-click file ? Copy Path

**Properties to Serialize:**
```csharp
// Look for properties with [Inputs] or [FindMissing] attributes
[Inputs("Output Path")]
[FindMissing]
public string OutputPath { get; set; }

[Inputs("Overwrite")]
public bool Overwrite { get; set; }
```

#### 1.2 Choose Constant Prefix

Rules:
- All UPPERCASE
- No spaces or special characters
- Typically related to activity function
- Examples: `FILEWRITE`, `PATHCREATE`, `DATAMERGE`

For `DsfFileWrite` ? Choose `FILEWRITE`

#### 1.3 Fill Configuration Table

Create your configuration:

```markdown
## Activity Configuration
- **ActivityClassName**: `DsfFileWrite`
- **ActivityFilePath**: `Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs`
- **DisplayName**: `"Write File"`
- **ConstantPrefix**: `FILEWRITE`

### Activity Properties to Serialize
| Property Name | C# Type | Constant Name | Default Value | Notes |
|---------------|---------|---------------|---------------|-------|
| OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty | Path to write file |
| Overwrite | bool | FILEWRITE_OVERWRITE | false | Overwrite if exists |
| AppendTop | bool | FILEWRITE_APPENDTOP | false | Append at top |
| AppendBottom | bool | FILEWRITE_APPENDBOTTOM | false | Append at bottom |
| Contents | string | FILEWRITE_CONTENTS | string.Empty | Content to write |
```

---

### Phase 2: Implementation (15-20 minutes)

#### Step 1: Add Constants

**File:** `Dev2.Common\X6\X6Models.cs`

**Location:** Inside the `Constants` class, add near similar constants

**What to add:**
```csharp
// Find existing similar constants like DSFPATHCREATE, then add:
public const string DSFFILEWRITE = "DsfFileWrite";
public const string DISPLAYNAME_FILEWRITE = "Write File";

// Add property constants (group them together)
public const string FILEWRITE_OUTPUTPATH = "outputpath";
public const string FILEWRITE_OVERWRITE = "overwrite";
public const string FILEWRITE_APPENDTOP = "appendtop";
public const string FILEWRITE_APPENDBOTTOM = "appendbottom";
public const string FILEWRITE_CONTENTS = "contents";
```

**Tips:**
- Keep constants alphabetically organized
- Group related constants together
- Property constant values are always lowercase

---

#### Step 2: Add Using Statements

**File:** Your activity file (e.g., `DsfFileWrite.cs`)

**Location:** Top of file with other using statements

**What to add:**
```csharp
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
```

---

#### Step 3: Implement ToX6Json

**File:** Your activity file

**Location:** At the end, before the closing brace of the class

**Template with replacements:**
```csharp
public override void ToX6Json(Cell cell)
{
    if (cell.data == null) cell.data = new Dictionary<string, object>();

    base.ToX6Json(cell);

    // Replace FILEWRITE with your ConstantPrefix
    cell.shape = Constants.DSFFILEWRITE;
    cell.data[Constants.TYPE] = Constants.DSFFILEWRITE.ToLower();
    cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_FILEWRITE;
    cell.data[Constants.UNIQUEID] = UniqueID;

    // Add one line per property from your configuration table
    // Format: cell.data.TryAdd(Constants.{PREFIX}_{PROPERTY}, {PropertyName});
    cell.data.TryAdd(Constants.FILEWRITE_OUTPUTPATH, OutputPath);
    cell.data.TryAdd(Constants.FILEWRITE_OVERWRITE, Overwrite);
    cell.data.TryAdd(Constants.FILEWRITE_APPENDTOP, AppendTop);
    cell.data.TryAdd(Constants.FILEWRITE_APPENDBOTTOM, AppendBottom);
    cell.data.TryAdd(Constants.FILEWRITE_CONTENTS, Contents);
    cell.data.TryAdd(Constants.RESULT, Result);
}
```

**How to fill in property serialization:**
1. Look at your properties table
2. For each property, add one `cell.data.TryAdd()` line
3. Use `Constants.{PREFIX}_{PROPERTY}` for the constant
4. Use `{PropertyName}` (PascalCase) for the value

---

#### Step 4: Implement FromX6Json

**File:** Your activity file

**Location:** Right after ToX6Json method

**Template with replacements:**
```csharp
public override void FromX6Json(Cell cell)
{
    if (cell == null || cell.data == null) return;

    base.FromX6Json(cell);

    if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) 
        DisplayName = displayName;
    if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) 
        UniqueID = uniqueId;

    // Add deserialization for each property
    // Choose TryGet method based on type: TryGetString, TryGetBool, TryGetInt
    if (cell.data.TryGetString(Constants.FILEWRITE_OUTPUTPATH, out var outputPath)) 
        OutputPath = outputPath;
    if (cell.data.TryGetBool(Constants.FILEWRITE_OVERWRITE, out var overwrite)) 
        Overwrite = overwrite;
    if (cell.data.TryGetBool(Constants.FILEWRITE_APPENDTOP, out var appendTop)) 
        AppendTop = appendTop;
    if (cell.data.TryGetBool(Constants.FILEWRITE_APPENDBOTTOM, out var appendBottom)) 
        AppendBottom = appendBottom;
    if (cell.data.TryGetString(Constants.FILEWRITE_CONTENTS, out var contents)) 
        Contents = contents;
    if (cell.data.TryGetString(Constants.RESULT, out var result)) 
        Result = result;

    // Defensive initialization for required string properties
    OutputPath ??= string.Empty;
    Contents ??= string.Empty;
}
```

**Type-specific TryGet methods:**
- `string` ? `TryGetString`
- `bool` ? `TryGetBool`
- `int` ? `TryGetInt`
- `decimal` ? `TryGetDecimal`

---

#### Step 5: Create WorkflowToX6Converter Helper

**File:** Create new file `Dev2.Activities\WorkflowConverters\WorkflowToX6Converter_FileWriteActivityHelper.cs`

**Naming pattern:** `WorkflowToX6Converter_{ActivityName}Helper.cs`
- Remove "Dsf" from class name
- Add "Activity" suffix
- Example: `DsfFileWrite` ? `FileWriteActivity`

**Content:**
```csharp
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateFileWriteActivity(DsfFileWrite activityInstance, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = activityInstance.DisplayName ?? Constants.DISPLAYNAME_FILEWRITE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;
            activityInstance.ToX6Json(cell);
            return cell;
        }
    }
}
```

**Replacements:**
- `CreateFileWriteActivity` ? `Create{ActivityName}`
- `DsfFileWrite` ? `{ActivityClassName}`
- `DISPLAYNAME_FILEWRITE` ? `DISPLAYNAME_{ConstantPrefix}`

---

#### Step 6: Create X6ToWorkflowConverter Helper

**File:** Create new file `Dev2.Activities\WorkflowConverters\X6ToWorkflowConverter_FileWriteActivityHelper.cs`

**Naming pattern:** Same as Step 5 but `X6ToWorkflowConverter_` prefix

**Content:**
```csharp
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfFileWrite CreateFileWriteActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfFileWrite();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
```

**Replacements:**
- `DsfFileWrite` ? `{ActivityClassName}`
- `CreateFileWriteActivity` ? `Create{ActivityName}`

---

#### Step 7: Integrate into WorkflowToX6Converter

**File:** `Dev2.Activities\WorkflowConverters\WorkflowToX6Converter.cs`

**Location:** Find the `CreateActivityNode` method, look for the long chain of `else if` statements

**Find this section:**
```csharp
else if (activity is DsfPathCreate pathCreateActivity)
{
    cell = CreatePathCreateActivity(pathCreateActivity, nodeId);
}
else
{
    cell.shape = Constants.RECT;
}
```

**Add your code BEFORE the final `else` block:**
```csharp
else if (activity is DsfFileWrite activityInstance)
{
    cell = CreateFileWriteActivity(activityInstance, nodeId);
}
```

---

#### Step 8: Integrate into X6ToWorkflowConverter

**File:** `Dev2.Activities\WorkflowConverters\X6ToWorkflowConverter.cs`

**Location:** Find the `CreateActivityFromNode` method with the large switch statement

**Find this section:**
```csharp
case var t when t.Contains(Constants.DSFPATHCREATE, StringComparison.OrdinalIgnoreCase):
    return CreatePathCreateActivity(node);
default:
    return new WriteLine { Text = "Unknown type" };
```

**Add your code BEFORE the `default:` case:**
```csharp
case var t when t.Contains(Constants.DSFFILEWRITE, StringComparison.OrdinalIgnoreCase):
    return CreateFileWriteActivity(node);
```

---

#### Step 9: Build and Verify

**Build:**
```bash
dotnet build
```

**Check for:**
- ? Zero compilation errors
- ? All new files are included in project
- ? Constants are accessible (IntelliSense shows them)

---

## Usage Methods

### Method 1: Manual Copy-Paste (Best for learning)

**Time:** 20-25 minutes  
**Skill Level:** Beginner

**Steps:**
1. Open `X6ConversionTemplate.md`
2. Fill in configuration section
3. Use Find & Replace in your editor:
   - Find: `{ActivityClassName}` ? Replace: Your value
   - Find: `{ConstantPrefix}` ? Replace: Your value
   - Find: `{DisplayName}` ? Replace: Your value
4. Copy code snippets to appropriate files
5. Build and test

**Pros:** Full control, learn the pattern  
**Cons:** More time-consuming, error-prone

---

### Method 2: AI-Assisted (Recommended)

**Time:** 5-10 minutes  
**Skill Level:** Any

**Steps:**
1. Fill in the configuration section in template
2. Use the prompt from `X6ConversionPrompt.md`
3. Provide configuration to AI
4. Review and apply generated code

**Pros:** Fast, accurate, minimal manual work  
**Cons:** Requires AI access

---

### Method 3: PowerShell Script (For bulk conversions)

**Time:** 2-3 minutes per activity after setup  
**Skill Level:** Intermediate

**Create script:** `Generate-X6Conversion.ps1`

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ActivityClassName,
    
    [Parameter(Mandatory=$true)]
    [string]$ActivityFilePath,
    
    [Parameter(Mandatory=$true)]
    [string]$DisplayName,
    
    [Parameter(Mandatory=$true)]
    [string]$ConstantPrefix,
    
    [Parameter(Mandatory=$true)]
    [hashtable]$Properties
)

# Derive ActivityName
$ActivityName = $ActivityClassName -replace '^Dsf', ''
if (-not $ActivityName.EndsWith('Activity')) {
    $ActivityName += 'Activity'
}

Write-Host "`n=== X6 Conversion Code Generator ===" -ForegroundColor Cyan
Write-Host "`nConfiguration:" -ForegroundColor Yellow
Write-Host "  ActivityClassName: $ActivityClassName"
Write-Host "  ActivityName: $ActivityName"
Write-Host "  ConstantPrefix: $ConstantPrefix"
Write-Host "  DisplayName: $DisplayName"

# Generate constants
Write-Host "`n--- Constants to add to X6Models.cs ---" -ForegroundColor Green
Write-Host "public const string DSF$ConstantPrefix = `"$ActivityClassName`";"
Write-Host "public const string DISPLAYNAME_$ConstantPrefix = `"$DisplayName`";"
Write-Host ""

foreach ($prop in $Properties.GetEnumerator()) {
    $propName = $prop.Key
    $constName = $prop.Value.Constant
    $propLower = $propName.ToLower()
    Write-Host "public const string $constName = `"$propLower`";"
}

# Generate ToX6Json code
Write-Host "`n--- ToX6Json method ---" -ForegroundColor Green
Write-Host "public override void ToX6Json(Cell cell)"
Write-Host "{"
Write-Host "    if (cell.data == null) cell.data = new Dictionary<string, object>();"
Write-Host ""
Write-Host "    base.ToX6Json(cell);"
Write-Host ""
Write-Host "    cell.shape = Constants.DSF$ConstantPrefix;"
Write-Host "    cell.data[Constants.TYPE] = Constants.DSF$ConstantPrefix.ToLower();"
Write-Host "    cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_$ConstantPrefix;"
Write-Host "    cell.data[Constants.UNIQUEID] = UniqueID;"
Write-Host ""

foreach ($prop in $Properties.GetEnumerator()) {
    $propName = $prop.Key
    $constName = $prop.Value.Constant
    Write-Host "    cell.data.TryAdd(Constants.$constName, $propName);"
}
Write-Host "    cell.data.TryAdd(Constants.RESULT, Result);"
Write-Host "}"

# Generate FromX6Json code
Write-Host "`n--- FromX6Json method ---" -ForegroundColor Green
Write-Host "public override void FromX6Json(Cell cell)"
Write-Host "{"
Write-Host "    if (cell == null || cell.data == null) return;"
Write-Host ""
Write-Host "    base.FromX6Json(cell);"
Write-Host ""
Write-Host "    if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName))"
Write-Host "        DisplayName = displayName;"
Write-Host "    if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId))"
Write-Host "        UniqueID = uniqueId;"
Write-Host ""

foreach ($prop in $Properties.GetEnumerator()) {
    $propName = $prop.Key
    $propType = $prop.Value.Type
    $constName = $prop.Value.Constant
    $varName = $propName.Substring(0,1).ToLower() + $propName.Substring(1)
    
    $tryGetMethod = switch ($propType) {
        "string" { "TryGetString" }
        "bool" { "TryGetBool" }
        "int" { "TryGetInt" }
        "decimal" { "TryGetDecimal" }
        default { "TryGetString" }
    }
    
    Write-Host "    if (cell.data.$tryGetMethod(Constants.$constName, out var $varName))"
    Write-Host "        $propName = $varName;"
}

Write-Host "    if (cell.data.TryGetString(Constants.RESULT, out var result))"
Write-Host "        Result = result;"
Write-Host ""

# Defensive initialization
foreach ($prop in $Properties.GetEnumerator()) {
    if ($prop.Value.Type -eq "string" -and $prop.Value.Default) {
        Write-Host "    $($prop.Key) ??= $($prop.Value.Default);"
    }
}
Write-Host "}"

Write-Host "`n=== Generation Complete ===" -ForegroundColor Cyan
```

**Usage:**
```powershell
.\Generate-X6Conversion.ps1 `
    -ActivityClassName "DsfFileWrite" `
    -ActivityFilePath "Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs" `
    -DisplayName "Write File" `
    -ConstantPrefix "FILEWRITE" `
    -Properties @{
        OutputPath = @{ Type = "string"; Constant = "FILEWRITE_OUTPUTPATH"; Default = "string.Empty" }
        Overwrite = @{ Type = "bool"; Constant = "FILEWRITE_OVERWRITE" }
        AppendTop = @{ Type = "bool"; Constant = "FILEWRITE_APPENDTOP" }
        AppendBottom = @{ Type = "bool"; Constant = "FILEWRITE_APPENDBOTTOM" }
        Contents = @{ Type = "string"; Constant = "FILEWRITE_CONTENTS"; Default = "string.Empty" }
    }
```

---

## Real Examples

### Example 1: Simple Activity (DsfPathCreate)

**Already Implemented - Use as Reference**

**Configuration:**
```markdown
- ActivityClassName: `DsfPathCreate`
- DisplayName: `"Create"`
- ConstantPrefix: `PATHCREATE`
- Properties: OutputPath (string), Overwrite (bool)
```

**Files to Review:**
- `Dev2.Common\X6\X6Models.cs` - Search for `PATHCREATE`
- `Dev2.Activities\Activities\PathOperations\DsfPathCreate.cs` - See ToX6Json/FromX6Json
- `Dev2.Activities\WorkflowConverters\WorkflowToX6Converter_PathCreateActivityHelper.cs`
- `Dev2.Activities\WorkflowConverters\X6ToWorkflowConverter_PathCreateActivityHelper.cs`

---

### Example 2: Medium Complexity (DsfFolderReadActivity)

**Already Implemented - Use as Reference**

**Configuration:**
```markdown
- ActivityClassName: `DsfFolderReadActivity`
- DisplayName: `"Folder Read"`
- ConstantPrefix: `FOLDERREAD`
- Properties: 
  - InputPath (string)
  - IsFilesSelected (bool)
  - IsFoldersSelected (bool)
  - IsFilesAndFoldersSelected (bool)
```

**Files to Review:**
- Search for `FOLDERREAD` in X6Models.cs
- Review `DsfFolderReadActivity.cs` for implementation pattern

---

### Example 3: New Activity (DsfFileWrite) - Step by Step

**Step-by-step walkthrough:**

1. **Prepare Configuration:**
```markdown
- ActivityClassName: `DsfFileWrite`
- ActivityFilePath: `Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs`
- DisplayName: `"Write File"`
- ConstantPrefix: `FILEWRITE`

Properties:
| OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty |
| Overwrite | bool | FILEWRITE_OVERWRITE | false |
| AppendTop | bool | FILEWRITE_APPENDTOP | false |
| AppendBottom | bool | FILEWRITE_APPENDBOTTOM | false |
| Contents | string | FILEWRITE_CONTENTS | string.Empty |
```

2. **Add Constants** (X6Models.cs):
```csharp
public const string DSFFILEWRITE = "DsfFileWrite";
public const string DISPLAYNAME_FILEWRITE = "Write File";
public const string FILEWRITE_OUTPUTPATH = "outputpath";
public const string FILEWRITE_OVERWRITE = "overwrite";
public const string FILEWRITE_APPENDTOP = "appendtop";
public const string FILEWRITE_APPENDBOTTOM = "appendbottom";
public const string FILEWRITE_CONTENTS = "contents";
```

3. **Implement methods in DsfFileWrite.cs** (see Step 3 & 4 in template)

4. **Create helper files** (see Step 5 & 6 in template)

5. **Integrate** (see Step 7 & 8 in template)

6. **Build and test**

---

## Troubleshooting

### Common Errors

#### Error: "Constants.DSFFILEWRITE does not exist"
**Cause:** Forgot to add constant or build hasn't recognized it  
**Solution:** 
1. Verify constant is in X6Models.cs
2. Rebuild project
3. Restart Visual Studio if needed

#### Error: "TryGetString is not defined"
**Cause:** Missing using statement  
**Solution:** Add `using Dev2.WorkflowConverters;` to activity file

#### Error: "The name 'CreateFileWriteActivity' does not exist"
**Cause:** Helper file not created or not in project  
**Solution:**
1. Verify helper file exists
2. Check it's included in Dev2.Activities project
3. Rebuild

#### Error: "Ambiguous match found" when building
**Cause:** Method defined twice  
**Solution:** Check for duplicate integration code in WorkflowToX6Converter.cs or X6ToWorkflowConverter.cs

### Build Warnings

#### Warning: "Cell.data may be null"
**Cause:** Normal, handled by null check  
**Solution:** Can be ignored or suppressed

### Testing Your Implementation

**Create a test workflow:**
1. Open Warewolf Studio
2. Create new workflow
3. Add your activity
4. Set properties
5. Save workflow
6. Export to X6 JSON
7. Verify JSON contains your properties
8. Import back from X6 JSON
9. Verify properties restored correctly

---

## Best Practices

### ? Do:
- Follow naming conventions exactly
- Test with real workflows
- Review existing implementations as examples
- Keep constants organized alphabetically
- Use defensive initialization for required properties
- Comment complex serialization logic

### ? Don't:
- Serialize inherited base properties (handled by base class)
- Skip defensive initialization
- Mix up constant casing
- Forget StringComparison.OrdinalIgnoreCase
- Skip building between steps (catch errors early)

---

## Getting Help

**Resources:**
1. Template file: `Docs/X6ConversionTemplate.md`
2. This usage guide: `Docs/X6ConversionTemplateUsage.md`
3. Prompt file: `Docs/X6ConversionPrompt.md`
4. Existing implementations: Search for `ToX6Json` in codebase

**Example implementations to study:**
- Simple: `DsfPathCreate`
- Medium: `DsfFolderReadActivity`
- Complex: `DsfDataMergeActivity`

**Still stuck?**
- Review the template step-by-step
- Compare with working example
- Check build output for specific errors
- Use AI assistant with the prompt file

# X6 JSON Conversion Implementation Template

## Activity Configuration
<!-- Fill in these values for your specific activity -->

**Activity Details:**
- **ActivityClassName**: `[FILL_IN]` _(Example: `DsfFileWrite`)_
- **ActivityFilePath**: `[FILL_IN]` _(Example: `Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs`)_
- **DisplayName**: `[FILL_IN]` _(Example: `"Write File"`)_
- **ConstantPrefix**: `[FILL_IN]` _(Example: `FILEWRITE` - all uppercase, used for constant names)_

### Activity Properties to Serialize
<!-- List all activity-specific properties that need to be serialized to X6 JSON -->
<!-- Do NOT include inherited base class properties (DisplayName, UniqueID, Result, Username, Password, PrivateKeyFile) -->

| Property Name | C# Type | Constant Name | Default Value | Notes |
|---------------|---------|---------------|---------------|-------|
| _[FILL_IN]_ | string/bool/int/decimal | _[FILL_IN]_ | _[FILL_IN]_ | _Optional notes_ |
| _[FILL_IN]_ | string/bool/int/decimal | _[FILL_IN]_ | _[FILL_IN]_ | _Optional notes_ |

**Example Configuration:**
```markdown
| OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty | File path to write to |
| Overwrite | bool | FILEWRITE_OVERWRITE | false | Whether to overwrite existing file |
| AppendTop | bool | FILEWRITE_APPENDTOP | false | Append content at top |
| AppendBottom | bool | FILEWRITE_APPENDBOTTOM | false | Append content at bottom |
| Contents | string | FILEWRITE_CONTENTS | string.Empty | Content to write |
```

---

## Implementation Steps

### Step 1: Add Constants to `Dev2.Common\X6\X6Models.cs`

**Location:** Inside the `Constants` class

**Add these constants:**

```csharp
// Activity type constant
public const string DSF{ConstantPrefix} = "{ActivityClassName}";

// Display name constant
public const string DISPLAYNAME_{ConstantPrefix} = "{DisplayName}";

// Property constants (add one for each property in the configuration table)
public const string {ConstantPrefix}_{PROPERTYNAME} = "{propertyname}";
```

**Token Mapping:**
- `{ConstantPrefix}` ? Value from Activity Configuration (e.g., `FILEWRITE`)
- `{ActivityClassName}` ? Full class name (e.g., `DsfFileWrite`)
- `{DisplayName}` ? Display name string (e.g., `"Write File"`)
- `{PROPERTYNAME}` ? Property name in UPPERCASE (e.g., `OUTPUTPATH`)
- `{propertyname}` ? Property name in lowercase (e.g., `outputpath`)

**Example:**
```csharp
public const string DSFFILEWRITE = "DsfFileWrite";
public const string DISPLAYNAME_FILEWRITE = "Write File";
public const string FILEWRITE_OUTPUTPATH = "outputpath";
public const string FILEWRITE_OVERWRITE = "overwrite";
public const string FILEWRITE_APPENDTOP = "appendtop";
public const string FILEWRITE_APPENDBOTTOM = "appendbottom";
public const string FILEWRITE_CONTENTS = "contents";
```

---

### Step 2: Add Required Using Statements to Activity Class

**Location:** At the top of `{ActivityFilePath}`

**Add these using statements:**
```csharp
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
```

---

### Step 3: Implement ToX6Json Method in Activity Class

**Location:** In the activity class file (`{ActivityFilePath}`), add at the end before closing brace

**Method Template:**
```csharp
public override void ToX6Json(Cell cell)
{
    if (cell.data == null) cell.data = new Dictionary<string, object>();

    base.ToX6Json(cell);

    cell.shape = Constants.DSF{ConstantPrefix};
    cell.data[Constants.TYPE] = Constants.DSF{ConstantPrefix}.ToLower();
    cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_{ConstantPrefix};
    cell.data[Constants.UNIQUEID] = UniqueID;

    // Property serialization - add one line for each property
    // For string properties:
    cell.data.TryAdd(Constants.{ConstantPrefix}_{PROPERTYNAME}, {PropertyName});
    
    // For bool properties:
    cell.data.TryAdd(Constants.{ConstantPrefix}_{PROPERTYNAME}, {PropertyName});
    
    // For int properties:
    cell.data.TryAdd(Constants.{ConstantPrefix}_{PROPERTYNAME}, {PropertyName});
    
    // For decimal properties:
    cell.data.TryAdd(Constants.{ConstantPrefix}_{PROPERTYNAME}, {PropertyName});
    
    // Always include Result property
    cell.data.TryAdd(Constants.RESULT, Result);
}
```

**Token Mapping:**
- `{ConstantPrefix}` ? e.g., `FILEWRITE`
- `{PROPERTYNAME}` ? Property name in UPPERCASE (e.g., `OUTPUTPATH`)
- `{PropertyName}` ? Property name in PascalCase (e.g., `OutputPath`)

**Example:**
```csharp
public override void ToX6Json(Cell cell)
{
    if (cell.data == null) cell.data = new Dictionary<string, object>();

    base.ToX6Json(cell);

    cell.shape = Constants.DSFFILEWRITE;
    cell.data[Constants.TYPE] = Constants.DSFFILEWRITE.ToLower();
    cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_FILEWRITE;
    cell.data[Constants.UNIQUEID] = UniqueID;

    cell.data.TryAdd(Constants.FILEWRITE_OUTPUTPATH, OutputPath);
    cell.data.TryAdd(Constants.FILEWRITE_OVERWRITE, Overwrite);
    cell.data.TryAdd(Constants.FILEWRITE_APPENDTOP, AppendTop);
    cell.data.TryAdd(Constants.FILEWRITE_APPENDBOTTOM, AppendBottom);
    cell.data.TryAdd(Constants.FILEWRITE_CONTENTS, Contents);
    cell.data.TryAdd(Constants.RESULT, Result);
}
```

---

### Step 4: Implement FromX6Json Method in Activity Class

**Location:** In the same activity class file, add after ToX6Json method

**Method Template:**
```csharp
public override void FromX6Json(Cell cell)
{
    if (cell == null || cell.data == null) return;

    base.FromX6Json(cell);

    if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) 
        DisplayName = displayName;
    if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) 
        UniqueID = uniqueId;

    // Property deserialization - add appropriate lines based on type
    
    // For string properties:
    if (cell.data.TryGetString(Constants.{ConstantPrefix}_{PROPERTYNAME}, out var {propertyName})) 
        {PropertyName} = {propertyName};
    
    // For bool properties:
    if (cell.data.TryGetBool(Constants.{ConstantPrefix}_{PROPERTYNAME}, out var {propertyName})) 
        {PropertyName} = {propertyName};
    
    // For int properties:
    if (cell.data.TryGetInt(Constants.{ConstantPrefix}_{PROPERTYNAME}, out var {propertyName})) 
        {PropertyName} = {propertyName};
    
    // For decimal properties:
    if (cell.data.TryGetDecimal(Constants.{ConstantPrefix}_{PROPERTYNAME}, out var {propertyName})) 
        {PropertyName} = {propertyName};

    // Result property
    if (cell.data.TryGetString(Constants.RESULT, out var result)) 
        Result = result;

    // Defensive initialization for required string properties
    {PropertyName} ??= string.Empty;
}
```

**Token Mapping:**
- `{ConstantPrefix}` ? e.g., `FILEWRITE`
- `{PROPERTYNAME}` ? Property name in UPPERCASE (e.g., `OUTPUTPATH`)
- `{PropertyName}` ? Property name in PascalCase (e.g., `OutputPath`)
- `{propertyName}` ? Property name in camelCase (e.g., `outputPath`)

**Example:**
```csharp
public override void FromX6Json(Cell cell)
{
    if (cell == null || cell.data == null) return;

    base.FromX6Json(cell);

    if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) 
        DisplayName = displayName;
    if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) 
        UniqueID = uniqueId;

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

    OutputPath ??= string.Empty;
    Contents ??= string.Empty;
}
```

---

### Step 5: Create WorkflowToX6Converter Helper File

**File Name:** `Dev2.Activities\WorkflowConverters\WorkflowToX6Converter_{ActivityName}Helper.cs`

**Token Mapping:**
- `{ActivityName}` ? ActivityClassName without "Dsf" prefix + "Activity" (e.g., `FileWriteActivity`)

**File Content Template:**
```csharp
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell Create{ActivityName}({ActivityClassName} activityInstance, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = activityInstance.DisplayName ?? Constants.DISPLAYNAME_{ConstantPrefix},
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;
            activityInstance.ToX6Json(cell);
            return cell;
        }
    }
}
```

**Example:**
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

---

### Step 6: Create X6ToWorkflowConverter Helper File

**File Name:** `Dev2.Activities\WorkflowConverters\X6ToWorkflowConverter_{ActivityName}Helper.cs`

**File Content Template:**
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
        private static {ActivityClassName} Create{ActivityName}(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new {ActivityClassName}();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
```

**Example:**
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

---

### Step 7: Integrate into WorkflowToX6Converter Main File

**File:** `Dev2.Activities\WorkflowConverters\WorkflowToX6Converter.cs`

**Location:** In the `CreateActivityNode` method, add **before** the final `else { cell.shape = Constants.RECT; }` block

**Code to Add:**
```csharp
else if (activity is {ActivityClassName} activityInstance)
{
    cell = Create{ActivityName}(activityInstance, nodeId);
}
```

**Example:**
```csharp
else if (activity is DsfFileWrite activityInstance)
{
    cell = CreateFileWriteActivity(activityInstance, nodeId);
}
```

---

### Step 8: Integrate into X6ToWorkflowConverter Main File

**File:** `Dev2.Activities\WorkflowConverters\X6ToWorkflowConverter.cs`

**Location:** In the `CreateActivityFromNode` switch statement, add **before** the `default:` case

**Code to Add:**
```csharp
case var t when t.Contains(Constants.DSF{ConstantPrefix}, StringComparison.OrdinalIgnoreCase):
    return Create{ActivityName}(node);
```

**Example:**
```csharp
case var t when t.Contains(Constants.DSFFILEWRITE, StringComparison.OrdinalIgnoreCase):
    return CreateFileWriteActivity(node);
```

---

### Step 9: Build and Verify

**Run build command:**
```bash
dotnet build
```

**Verify:**
- ? No compilation errors
- ? All files are saved
- ? Constants are properly defined
- ? Methods are implemented correctly
- ? Integration points are updated

---

## Token Reference Quick Guide

| Token | Description | Example |
|-------|-------------|---------|
| `{ActivityClassName}` | Full class name from config | `DsfFileWrite` |
| `{ActivityFilePath}` | File path from config | `Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs` |
| `{DisplayName}` | Display name from config | `"Write File"` |
| `{ConstantPrefix}` | Constant prefix from config (UPPERCASE) | `FILEWRITE` |
| `{ActivityName}` | Class name without "Dsf" + "Activity" | `FileWriteActivity` |
| `{PROPERTYNAME}` | Property name in UPPERCASE | `OUTPUTPATH` |
| `{propertyname}` | Property name in lowercase | `outputpath` |
| `{PropertyName}` | Property name in PascalCase | `OutputPath` |
| `{propertyName}` | Property name in camelCase | `outputPath` |

---

## TryGet Methods by Type

| C# Type | TryGet Method | Example |
|---------|---------------|---------|
| `string` | `TryGetString` | `cell.data.TryGetString(Constants.FILEWRITE_PATH, out var path)` |
| `bool` | `TryGetBool` | `cell.data.TryGetBool(Constants.FILEWRITE_OVERWRITE, out var overwrite)` |
| `int` | `TryGetInt` | `cell.data.TryGetInt(Constants.FILEWRITE_TIMEOUT, out var timeout)` |
| `decimal` | `TryGetDecimal` | `cell.data.TryGetDecimal(Constants.FILEWRITE_SIZE, out var size)` |

---

## Naming Conventions Summary

**Constants:**
- Activity Type: `DSF{ConstantPrefix}` (e.g., `DSFFILEWRITE`)
- Display Name: `DISPLAYNAME_{ConstantPrefix}` (e.g., `DISPLAYNAME_FILEWRITE`)
- Properties: `{ConstantPrefix}_{PROPERTYNAME}` (e.g., `FILEWRITE_OUTPUTPATH`)

**File Names:**
- Workflow To X6: `WorkflowToX6Converter_{ActivityName}Helper.cs`
- X6 To Workflow: `X6ToWorkflowConverter_{ActivityName}Helper.cs`

**Method Names:**
- Create Method: `Create{ActivityName}` (e.g., `CreateFileWriteActivity`)

---

## Common Pitfalls to Avoid

1. ? **Don't serialize inherited base class properties** - They're handled by `base.ToX6Json()`
2. ? **Don't forget defensive initialization** - Add `PropertyName ??= string.Empty;` for required string properties
3. ? **Don't mix up constant casing** - Constants are UPPERCASE, property names vary by context
4. ? **Don't skip the `StringComparison.OrdinalIgnoreCase`** - Required for case-insensitive matching
5. ? **Don't forget to call `base.FromX6Json(cell)`** - Must be first line after null check

---

## Template Version
**Version:** 1.0  
**Last Updated:** 2024  
**Compatible With:** Warewolf .NET 6 / C# 10.0

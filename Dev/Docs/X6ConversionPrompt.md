# X6 JSON Conversion Prompt Template

Use this prompt when working with AI assistants to implement X6 JSON conversion for Warewolf activities.

---

## Prompt Template

```
I need to implement X6 JSON conversion for a Warewolf activity following the established pattern.

ACTIVITY_CONFIG_START
ActivityClassName: [FILL_IN - e.g., DsfFileWrite]
ActivityFilePath: [FILL_IN - e.g., Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs]
DisplayName: [FILL_IN - e.g., "Write File"]
ConstantPrefix: [FILL_IN - e.g., FILEWRITE]
ActivityName: [FILL_IN - e.g., FileWriteActivity]

Properties:
[FILL_IN - One line per property in format: PropertyName | Type | CONSTANT_NAME | DefaultValue]
- [PropertyName] | [string/bool/int/decimal] | [CONSTANT_NAME] | [default value]

Example:
- OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty
- Overwrite | bool | FILEWRITE_OVERWRITE | false
- Contents | string | FILEWRITE_CONTENTS | string.Empty
ACTIVITY_CONFIG_END

Please implement the following:

1. Add constants to Dev2.Common\X6\X6Models.cs:
   - Activity type constant: DSF{ConstantPrefix} = "{ActivityClassName}"
   - Display name constant: DISPLAYNAME_{ConstantPrefix} = "{DisplayName}"
   - Property constants: {ConstantPrefix}_{PROPERTYNAME} = "{propertyname}" (lowercase)

2. Add ToX6Json method to the activity class:
   - Add using statements: Dev2.Common.X6, Dev2.WorkflowConverters
   - Serialize all properties listed above
   - Set shape and type constants
   - Include Result property

3. Add FromX6Json method to the activity class:
   - Deserialize all properties with appropriate TryGet methods
   - Use TryGetString for string, TryGetBool for bool, TryGetInt for int
   - Add defensive initialization for required string properties

4. Create WorkflowToX6Converter helper:
   - File: Dev2.Activities\WorkflowConverters\WorkflowToX6Converter_{ActivityName}Helper.cs
   - Implement Create{ActivityName} method
   - Set position and call ToX6Json

5. Create X6ToWorkflowConverter helper:
   - File: Dev2.Activities\WorkflowConverters\X6ToWorkflowConverter_{ActivityName}Helper.cs
   - Implement static Create{ActivityName} method
   - Validate displayName and call FromX6Json

6. Integrate into WorkflowToX6Converter.cs:
   - Add case in CreateActivityNode method before final else block
   - Pattern: else if (activity is {ActivityClassName} activityInstance) { cell = Create{ActivityName}(activityInstance, nodeId); }

7. Integrate into X6ToWorkflowConverter.cs:
   - Add case in CreateActivityFromNode switch before default case
   - Pattern: case var t when t.Contains(Constants.DSF{ConstantPrefix}, StringComparison.OrdinalIgnoreCase): return Create{ActivityName}(node);

Follow the exact pattern used in DsfPathCreate and DsfFolderReadActivity implementations.
Show complete code for all files.
```

---

## Usage Instructions

### Step 1: Fill in the Configuration

Replace all `[FILL_IN]` placeholders with your activity's information:

**Example:**
```
ACTIVITY_CONFIG_START
ActivityClassName: DsfFileWrite
ActivityFilePath: Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs
DisplayName: "Write File"
ConstantPrefix: FILEWRITE
ActivityName: FileWriteActivity

Properties:
- OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty
- Overwrite | bool | FILEWRITE_OVERWRITE | false
- AppendTop | bool | FILEWRITE_APPENDTOP | false
- AppendBottom | bool | FILEWRITE_APPENDBOTTOM | false
- Contents | string | FILEWRITE_CONTENTS | string.Empty
ACTIVITY_CONFIG_END
```

### Step 2: Copy Entire Prompt

Copy the complete prompt (including your filled configuration) to your AI assistant.

### Step 3: Review Generated Code

The AI will generate:
- Constants to add
- ToX6Json method
- FromX6Json method
- WorkflowToX6Converter helper file
- X6ToWorkflowConverter helper file
- Integration code snippets

### Step 4: Apply Changes

Apply each code snippet to the appropriate file in your codebase.

### Step 5: Build and Test

Run `dotnet build` to verify no errors.

---

## Quick Reference: Configuration Values

### How to Find Each Value

| Field | How to Find | Example |
|-------|-------------|---------|
| **ActivityClassName** | Class name from your activity file | `DsfFileWrite` |
| **ActivityFilePath** | Right-click file ? Copy Path | `Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs` |
| **DisplayName** | From `[ToolDescriptorInfo]` attribute | `"Write File"` |
| **ConstantPrefix** | Activity function in UPPERCASE | `FILEWRITE` |
| **ActivityName** | ClassName without "Dsf" + "Activity" | `FileWriteActivity` |
| **Properties** | Look for `[Inputs]` or `[FindMissing]` attributes | See below |

### Property Format

For each property in your activity:
```
PropertyName | Type | CONSTANT_NAME | DefaultValue
```

**Type options:**
- `string` - Use `TryGetString`
- `bool` - Use `TryGetBool`
- `int` - Use `TryGetInt`
- `decimal` - Use `TryGetDecimal`

**Constant naming:**
```
{ConstantPrefix}_{PROPERTYNAME_UPPERCASE}
```

**Example properties:**
```
- OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty
- Overwrite | bool | FILEWRITE_OVERWRITE | false
- Timeout | int | FILEWRITE_TIMEOUT | 30
- Size | decimal | FILEWRITE_SIZE | 0.0m
```

---

## Example Prompts

### Example 1: Simple Activity (DsfPathDelete)

```
I need to implement X6 JSON conversion for a Warewolf activity following the established pattern.

ACTIVITY_CONFIG_START
ActivityClassName: DsfPathDelete
ActivityFilePath: Dev2.Activities\Activities\PathOperations\DsfPathDelete.cs
DisplayName: "Delete"
ConstantPrefix: PATHDELETE
ActivityName: PathDeleteActivity

Properties:
- InputPath | string | PATHDELETE_INPUTPATH | string.Empty
ACTIVITY_CONFIG_END

[... rest of standard prompt ...]
```

### Example 2: Medium Complexity (DsfFileWrite)

```
I need to implement X6 JSON conversion for a Warewolf activity following the established pattern.

ACTIVITY_CONFIG_START
ActivityClassName: DsfFileWrite
ActivityFilePath: Dev2.Activities\Activities\PathOperations\DsfFileWrite.cs
DisplayName: "Write File"
ConstantPrefix: FILEWRITE
ActivityName: FileWriteActivity

Properties:
- OutputPath | string | FILEWRITE_OUTPUTPATH | string.Empty
- Overwrite | bool | FILEWRITE_OVERWRITE | false
- AppendTop | bool | FILEWRITE_APPENDTOP | false
- AppendBottom | bool | FILEWRITE_APPENDBOTTOM | false
- Contents | string | FILEWRITE_CONTENTS | string.Empty
- Encoding | string | FILEWRITE_ENCODING | UTF-8
ACTIVITY_CONFIG_END

[... rest of standard prompt ...]
```

### Example 3: With Integer Properties (DsfDatabaseTimeout)

```
I need to implement X6 JSON conversion for a Warewolf activity following the established pattern.

ACTIVITY_CONFIG_START
ActivityClassName: DsfDatabaseTimeout
ActivityFilePath: Dev2.Activities\Activities\Database\DsfDatabaseTimeout.cs
DisplayName: "Database Timeout"
ConstantPrefix: DATABASETIMEOUT
ActivityName: DatabaseTimeoutActivity

Properties:
- ConnectionString | string | DATABASETIMEOUT_CONNECTION | string.Empty
- CommandTimeout | int | DATABASETIMEOUT_TIMEOUT | 30
- RetryCount | int | DATABASETIMEOUT_RETRIES | 3
- EnablePooling | bool | DATABASETIMEOUT_POOLING | true
ACTIVITY_CONFIG_END

[... rest of standard prompt ...]
```

---

## Advanced Usage

### Adding Context for Complex Activities

If your activity has complex nested objects or special handling, add a note:

```
ACTIVITY_CONFIG_START
[... standard config ...]

SPECIAL_NOTES:
- The Settings property is a complex object that needs custom serialization
- The InputMappings collection should be serialized as a JSON array
- The SourceId references another resource and needs special handling
ACTIVITY_CONFIG_END
```

### Referencing Existing Implementation

To ask AI to match an existing pattern exactly:

```
Please implement X6 JSON conversion for DsfFileWrite following the EXACT pattern used in DsfPathCreate.

Reference implementation: Dev2.Activities\Activities\PathOperations\DsfPathCreate.cs

ACTIVITY_CONFIG_START
[... config ...]
ACTIVITY_CONFIG_END

Use the same structure, naming conventions, and code organization as DsfPathCreate.
```

---

## Verification Checklist

After AI generates code, verify:

- [ ] Constants follow naming convention: `DSF{PREFIX}`, `DISPLAYNAME_{PREFIX}`, `{PREFIX}_{PROPERTY}`
- [ ] Property constants are lowercase
- [ ] ToX6Json includes all properties
- [ ] FromX6Json uses correct TryGet methods for each type
- [ ] Helper files are named correctly: `WorkflowToX6Converter_{ActivityName}Helper.cs`
- [ ] Integration code uses `StringComparison.OrdinalIgnoreCase`
- [ ] Defensive initialization for required string properties
- [ ] Result property is included

---

## Troubleshooting AI Responses

### If AI forgets properties:
```
You missed the following properties: [list]. Please add them to both ToX6Json and FromX6Json methods.
```

### If AI uses wrong constant names:
```
The constant names should follow this pattern:
- Activity: DSF{ConstantPrefix} (all caps)
- Display: DISPLAYNAME_{ConstantPrefix}
- Properties: {ConstantPrefix}_{PROPERTYNAME}

Please regenerate with correct naming.
```

### If AI skips integration:
```
Please also provide the integration code for:
1. WorkflowToX6Converter.cs - CreateActivityNode method
2. X6ToWorkflowConverter.cs - CreateActivityFromNode method
```

---

## Tips for Best Results

1. **Be Specific:** Fill in ALL configuration fields completely
2. **Provide Context:** Mention similar activities as references
3. **Request Complete Code:** Ask for full file contents, not snippets
4. **Verify Types:** Double-check property types (string vs bool vs int)
5. **Check Defaults:** Ensure default values match your activity's constructor
6. **Review Constants:** Verify constant names follow the pattern exactly

---

## Template Version
**Version:** 1.0  
**Last Updated:** 2024  
**Compatible With:** Warewolf .NET 6 / C# 10.0  
**Tested With:** GitHub Copilot, ChatGPT, Claude

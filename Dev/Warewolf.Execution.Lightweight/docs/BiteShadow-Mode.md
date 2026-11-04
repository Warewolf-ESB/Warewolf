# BiteShadow mode: comparing the WF-free reader in production

Part of the Windows Workflow Foundation (WF) removal, task T1.7. This applies to both engines.

## What it does

With shadow mode on, every **fresh** workflow build is checked twice:

1. The workflow is built the usual way: `ActivityXamlServices.Load` + `ActivityParser`.
2. The same XAML is also read by the WF-free `Warewolf.Bite.BiteReader`.
3. The two activity graphs are compared property by property, and the outcome is logged.

**The WF-built graph is always the one that executes.** The new reader's output is only compared, never run. If the new reader or the comparison fails, the failure is logged and ignored, so an execution can't fail because of shadow mode.

"Fresh" means a build that isn't served from a cache:

- **Lightweight engine:** a workflow-pool miss in `WorkflowExecutor.BuildExclusivePreparedWorkflow`. That covers executions, tests and resumes.
- **Server:** an activity-cache miss in `ResourceCatalog.Parse`.

Cached builds cost nothing extra.

## Turning it on

Set this environment variable or app setting, then restart:

```
WAREWOLF_BITE_SHADOW=true
```

The value `1` also works. Anything else, or leaving it unset, means off.

There is no deploy-script parameter for it yet. Set it by hand for a soak, for example:

```powershell
az functionapp config appsettings set -g <rg> -n <app> --settings WAREWOLF_BITE_SHADOW=true
```

On start-up, the engine logs that the variable is on: "`WAREWOLF_BITE_SHADOW is on: workflows are shadow-read by the WF-free reader`".

## What gets logged

Each fresh build logs one line, through `Dev2Logger`. On the Lightweight engine that reaches App Insights through the composite logger.

| Outcome | Level | Example |
|---|---|---|
| Match | Info | `BiteShadow match <resource> (read 0.4 ms, compare 2.1 ms)` |
| Mismatch | Warn | `BiteShadow MISMATCH <resource>: 2 difference(s): nodes.n0003.Text [1a2b3c4d != 9f8e7d6c]; ...` |
| New reader failed | Warn | `BiteShadow candidate error <resource>: BiteFormatException: ...` |

How to read these lines:

- **`<resource>`** is the workflow file path on the Lightweight engine and the resource ID on the Server.
- **Differences are paths plus the first 8 hex digits of a SHA-256 of each value, never the values themselves.** Activity properties can hold connection strings and passwords. To investigate, load the named workflow locally and run the parity harness (`Dev2.Activities.Tests`, category `WfParity`), which shows full values.
- **Activities saved without a `UniqueID`** get an ID minted at load time on both readers. Those differences are ignored, so they never show up as mismatches.
- **Secrets encrypted again on load** (a password stored by another machine, or as plain text) come out as
  fresh DPAPI or `WFAES::` ciphertext on each read. When both sides are ciphertext and neither is the stored
  value, the difference is ignored (`ReencryptedSecrets`).

## App Insights query (Lightweight engine)

The logger prefixes each message with a correlation ID and an execution ID, so these queries use `has` rather than `startswith`.

```kusto
traces
| where message has "BiteShadow"
| extend outcome = case(message has "BiteShadow match", "match",
                        message has "BiteShadow MISMATCH", "mismatch", "error")
| summarize count() by outcome, bin(timestamp, 1h)
```

To list the mismatches:

```kusto
traces
| where message has "BiteShadow MISMATCH" or message has "BiteShadow candidate error"
| project timestamp, cloud_RoleInstance, message
| order by timestamp desc
```

## Cost

Each fresh build pays once for the new read and the comparison:

- The new read takes about 0.3 ms warm.
- The comparison takes a few milliseconds.
- The first read in a new process takes about 200 ms.

This happens synchronously, before the workflow is handed out. Comparing later would race with the execution changing the same activity objects.

## Exit criterion (T1.7)

Zero mismatches and zero candidate errors on UAT and `ashleyswarewolfserver`, over an agreed soak period. After that comes T1.8: making the new reader the default.

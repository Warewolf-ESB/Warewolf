---
name: warewolf-commit
description: Warewolf commit-message format and the never-auto-commit guardrail. Invoke whenever asked to create/produce a commit message or stage changes for commit. Covers the {branch-number}-#{Feature|Fix}-{branch-name} subject format, body bullet rules, and the model-named Co-Authored-By trailer.
---

# Commit messages

> **STRICTLY never auto-commit.** Do not run `git commit` (or `git push`) on your own under any circumstances. When asked to "create a commit message", only **stage** the relevant files and present the formatted message plus a changes summary as text — leave the actual commit to the user. Phrases like "go ahead" or "proceed" after a message request mean *produce the message*, not commit. Commit only when the user explicitly and unambiguously instructs you to commit.

## Format

`{branch-number}-#{Feature|Fix}-{remaining-branch-name}`

| Tag | When to use |
|---|---|
| `#Feature` | New feature, addition, or non-trivial edit that is not a defect fix |
| `#Fix` | Bug fix, build error fix, or test failure fix |

Example — branch `8433-Fix-Tests-Failure-Warewolf.Execution.Lightweight.Tests`, fixing build errors:

```
8433-#Fix-Fix-Tests-Failure-Warewolf.Execution.Lightweight.Tests

- Fixed CS7036 in ServiceCollectionExtensions.cs: AuditLogger DI registration changed from new AuditLogger() to AddSingleton<AuditLogger>()
- Fixed CS0535 in CoreInfrastructureTests.cs / WorkflowAuthPolicyTests.cs: added missing LogTrace stubs to NoOpLogger and TrackingLogger
- Fixed AuditLogger.LogKeyVaultError argument order: exception was passed as format arg, corrected to _logger.LogError(ex, message)
```

## Rules
- First line is the subject — branch number, tag, then the remaining branch name verbatim (no description of your own).
- Body bullet points summarise each logical change; one bullet per file or concern.
- Keep subject under 72 characters if possible; body lines under 100.
- End every commit message with a Co-Authored-By trailer on its own line, after a blank line following the body. The trailer **must name the actual Claude model used in the current session** (not a hard-coded value) — use the model's display name exactly as reported for the session:

  ```
  Co-Authored-By: <session Claude model display name> <noreply@anthropic.com>
  ```

  Example for a session running Opus 4.8 (1M context):

  ```
  Co-Authored-By: Claude Opus 4.8 (1M context) <noreply@anthropic.com>
  ```

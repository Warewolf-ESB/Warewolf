# @warewolf/engine

The Warewolf execution engine, self-contained (no .NET install needed), on Linux (x64, arm64) and
Windows (x64). npm installs only the engine for your platform.

## Run a workflow, no server

```sh
npm install -g @warewolf/engine
warewolf run hello.bite --set Name=Wolf
# { "Message": "Hello Wolf." }
```

`warewolf run` executes one workflow and exits. Nothing listens on a port and nothing is installed as a
service. It reads `.bite` files whose body is JSON (`<BiteDefinition FormatVersion="1">`, see
`Warewolf.Bite/docs/BiteDefinition-Format.md`) as well as XAML ones.

| Option | |
|---|---|
| `--input <json>` | The workflow's inputs. |
| `--set <name=value>` | One input; repeat for more. No JSON quoting, which `cmd.exe` and PowerShell mangle in npm's Windows shim. |
| `--input-file <path>` | Read the inputs from a file; `-` reads stdin. |
| `--resources <dir>` | The folder with the sources and workflows it calls. Defaults to the workflow's folder. |
| `--xml` | The output as XML instead of JSON. |
| `--debug` | The debug tree instead of the output. |

The output goes to stdout and errors to stderr, so it pipes. The exit code is `0` when the workflow
succeeded, `1` when it failed and `2` for a usage error. It logs nothing by default; to see the engine's
logs on stderr set `WAREWOLF_LOGGING_CONFIG='{"console":true}'` (and `EXECUTIONLOGLEVEL`).

From Node:

```js
import { runWorkflow } from "@warewolf/engine";

const { success, output, errors } = await runWorkflow("hello.bite", { input: { Name: "Wolf" } });
```

`runWorkflow` resolves whether or not the workflow failed; check `success`. It takes the same options
as the command line (`input`, `inputFile`, `resources`, `xml`, `debug`). `output` is the parsed JSON, or
the text with `xml`.

## Host it as a service

```sh
sudo warewolf-server install-service     # Linux: the warewolf-server systemd unit
warewolf-server install-service          # Windows, elevated: the "Warewolf Server" service
```

The engine then runs on `127.0.0.1:3142`. Workflows live in `/var/lib/warewolf/Resources` on Linux and
`%ProgramData%\Warewolf\Resources` on Windows. `warewolf-server run` runs it in the foreground instead.

A locally hosted or locally run engine is free: it doesn't check for a licence.

**Keep it on loopback.** The hosted engine runs without a secure.config, so anything that can reach port
3142 can run every workflow. It listens on `127.0.0.1` unless you set `WAREWOLF_SELFHOST_URLS`; don't open
the port in a firewall.

On Linux it needs `libicu` (`apt install libicu72`, `dnf install libicu`).

#!/usr/bin/env node
/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

// warewolf: runs a workflow and exits, without hosting a server. The arguments after `run <file>` go to the engine.
import { realpathSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { runWorkflowInherited } from "./index.mjs";

export const HELP = `warewolf — run Warewolf workflows without a server

  warewolf run <workflow.bite> [options]

  --input <json>        the workflow's inputs, as JSON
  --set <name=value>    one input; repeat for more (no JSON quoting, handy in cmd.exe and PowerShell)
  --input-file <path>   read the inputs from a file; '-' reads them from stdin
  --resources <dir>     the folder holding the sources and workflows it calls (default: the workflow's folder)
  --xml                 write the output as XML instead of JSON
  --debug               write the debug tree instead of the output

The workflow's output goes to stdout and its errors to stderr. The exit code is 0 when it succeeded,
1 when it failed and 2 for a usage error. It reads .bite files whose body is JSON (<BiteDefinition>) or XAML.
`;

/**
 * The engine's arguments for a command line, or null when it isn't a run. Each --set name=value becomes
 * one JSON input, so nothing has to be quoted through the npm shim on Windows, which strips quotes.
 * @throws {Error} a --set that isn't name=value, or one combined with --input or --input-file.
 */
export function engineArguments(argv) {
  const [command, file, ...rest] = argv;
  if (command !== "run" || !file) return null;

  const passed = [];
  const inputs = {};
  for (let i = 0; i < rest.length; i++) {
    if (rest[i].toLowerCase() !== "--set") {
      passed.push(rest[i]);
      continue;
    }
    const pair = rest[++i] ?? "";
    const at = pair.indexOf("=");
    if (at < 1) throw new Error(`--set needs name=value, not '${pair}'.`);
    inputs[pair.slice(0, at)] = pair.slice(at + 1);
  }

  if (Object.keys(inputs).length) {
    if (passed.some((a) => ["--input", "--input-file"].includes(a.toLowerCase()))) throw new Error("Use --set, --input or --input-file, not a mix.");
    passed.push("--input", JSON.stringify(inputs));
  }
  return ["--run", file, ...passed];
}

async function main(argv) {
  let args;
  try {
    args = engineArguments(argv);
  } catch (error) {
    console.error(`warewolf: ${error.message}`);
    process.exitCode = 2;
    return;
  }
  if (!args) {
    const help = !argv.length || ["help", "--help", "-h"].includes(argv[0]);
    (help ? console.log : console.error)(help ? HELP : `Unknown command or missing workflow file.\n\n${HELP}`);
    process.exitCode = help ? 0 : 2;
    return;
  }
  process.exitCode = await runWorkflowInherited(args);
}

if (process.argv[1] && fileURLToPath(import.meta.url) === realpathSync(process.argv[1])) {
  main(process.argv.slice(2)).catch((error) => {
    console.error(`warewolf: ${error instanceof Error ? error.message : String(error)}`);
    process.exit(1);
  });
}

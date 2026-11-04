/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

// Runs a workflow without hosting a server: the platform engine is started with --run, executes the .bite
// file (a JSON <BiteDefinition> body, or legacy XAML) and exits. Nothing listens on a port.
import { execFile, spawn } from "node:child_process";
import { enginePath, ensureExecutable } from "./cli.mjs";

/**
 * The engine's arguments for one run.
 * @param {string} file the .bite workflow
 * @param {{ input?: object|string, inputFile?: string, resources?: string, xml?: boolean, debug?: boolean }} [options]
 *   input: the workflow's inputs, an object (sent as JSON) or a JSON string.
 *   inputFile: read the inputs from this file instead ("-" reads stdin).
 *   resources: the folder with the sources and workflows it calls (default: the workflow's folder).
 *   xml: the output as XML instead of JSON. debug: the debug tree instead of the output.
 */
export function runArguments(file, { input, inputFile, resources, xml, debug } = {}) {
  if (!file) throw new Error("A workflow file is required.");
  if (input !== undefined && inputFile !== undefined) throw new Error("Use input or inputFile, not both.");
  const args = ["--run", file];
  if (input !== undefined) args.push("--input", typeof input === "string" ? input : JSON.stringify(input));
  if (inputFile !== undefined) args.push("--input-file", inputFile);
  if (resources !== undefined) args.push("--resources", resources);
  if (xml) args.push("--xml");
  if (debug) args.push("--debug");
  return args;
}

/**
 * Runs a workflow and resolves with its result; it never rejects for a workflow that fails.
 * @returns {Promise<{ success: boolean, exitCode: number, output: any, errors: string[] }>}
 *   output is the parsed JSON, or the text for xml. errors are the engine's messages.
 */
export function runWorkflow(file, options = {}) {
  const exe = enginePath();
  ensureExecutable(exe);
  const args = runArguments(file, options);
  return new Promise((resolve, reject) => {
    const child = execFile(exe, args, { encoding: "utf8", maxBuffer: 256 * 1024 * 1024 }, (error, stdout, stderr) => {
      if (error && typeof error.code !== "number") return reject(error); // the engine didn't start
      const exitCode = error ? error.code : 0;
      const text = stdout.trim();
      let output = text;
      if (!options.xml && text) {
        try {
          output = JSON.parse(text);
        } catch {
          // Not JSON (an engine message): hand back the text.
        }
      }
      resolve({ success: exitCode === 0, exitCode, output, errors: stderr.split(/\r?\n/).filter(Boolean) });
    });
    child.stdin.end();
  });
}

/** Runs a workflow with the engine's output and errors going straight to this process's stdout and stderr. Resolves with the exit code. */
export function runWorkflowInherited(args) {
  const exe = enginePath();
  ensureExecutable(exe);
  return new Promise((resolve, reject) => {
    const child = spawn(exe, args, { stdio: "inherit" });
    child.on("error", reject);
    child.on("exit", (code, signal) => resolve(signal ? 1 : (code ?? 1)));
  });
}

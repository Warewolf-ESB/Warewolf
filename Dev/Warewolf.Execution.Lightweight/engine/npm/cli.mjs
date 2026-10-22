#!/usr/bin/env node
/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

// warewolf-server: runs the self-contained engine from this machine's @warewolf/engine-<platform>
// package (npm installs only the one matching os/cpu), self-hosted on 127.0.0.1:3142
// (SelfHost/SelfHostedEngine.cs), and installs it as the "Warewolf Server" service.
import { execFileSync, spawn, spawnSync } from "node:child_process";
import { chmodSync, mkdirSync, realpathSync, rmSync, writeFileSync } from "node:fs";
import { createRequire } from "node:module";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

export const SERVICE_NAME = "Warewolf Server";
export const SYSTEMD_UNIT = "warewolf-server.service";
export const SERVICE_USER = "warewolf";
export const LINUX_DATA_DIR = "/var/lib/warewolf";
export const PLATFORMS = ["linux-x64", "linux-arm64", "win32-x64"];

const require = createRequire(import.meta.url);

/** The platform package this machine needs, e.g. @warewolf/engine-linux-x64. */
export function platformPackage(platform = process.platform, arch = process.arch) {
  const key = `${platform}-${arch}`;
  if (!PLATFORMS.includes(key)) throw new Error(`The Warewolf engine isn't built for ${key}. Supported: ${PLATFORMS.join(", ")}.`);
  return `@warewolf/engine-${key}`;
}

/** The engine executable inside the installed platform package. */
export function enginePath(platform = process.platform, arch = process.arch, resolve = require.resolve) {
  const name = platformPackage(platform, arch);
  let manifest;
  try {
    manifest = resolve(`${name}/package.json`);
  } catch {
    throw new Error(`${name} isn't installed. Reinstall @warewolf/engine without --no-optional (npm installs the platform package as an optional dependency).`);
  }
  return join(dirname(manifest), "bin", platform === "win32" ? "Warewolf.Execution.Lightweight.exe" : "Warewolf.Execution.Lightweight");
}

/** npm doesn't keep the executable bit from a tarball packed on Windows, so set it before running. */
export function ensureExecutable(exe, platform = process.platform) {
  if (platform === "win32") return;
  try {
    chmodSync(exe, 0o755);
  } catch {
    // Not ours to change (installed by root, run as a user): it was made executable at install-service.
  }
}

/** The systemd unit: the engine as the unprivileged warewolf user, writing only its data folder. */
export function systemdUnit({ exe, dataDir = LINUX_DATA_DIR, user = SERVICE_USER }) {
  return [
    "[Unit]",
    `Description=${SERVICE_NAME} (Warewolf execution engine)`,
    "After=network-online.target",
    "Wants=network-online.target",
    "",
    "[Service]",
    `ExecStart=${exe} --self-host`,
    `WorkingDirectory=${dirname(exe)}`,
    `User=${user}`,
    `Group=${user}`,
    `Environment=HOME=${dataDir}`,
    `Environment=WAREWOLF_DATA_DIR=${dataDir}`,
    "NoNewPrivileges=yes",
    "ProtectSystem=strict",
    "ProtectHome=yes",
    "PrivateTmp=yes",
    `ReadWritePaths=${dataDir}`,
    "Restart=on-failure",
    "RestartSec=5",
    "",
    "[Install]",
    "WantedBy=multi-user.target",
    "",
  ].join("\n");
}

function run(command, args) {
  execFileSync(command, args, { stdio: "inherit" });
}

function installService(exe) {
  if (process.platform === "win32") {
    if (spawnSync("net", ["session"], { stdio: "ignore" }).status !== 0) throw new Error("install-service registers a Windows service: run it from an elevated (Administrator) prompt.");
    run("sc.exe", ["create", SERVICE_NAME, "binPath=", `"${exe}" --self-host`, "start=", "auto", "DisplayName=", SERVICE_NAME]);
    run("sc.exe", ["description", SERVICE_NAME, "Warewolf execution engine, on 127.0.0.1:3142. Data in %ProgramData%\\Warewolf."]);
    run("sc.exe", ["start", SERVICE_NAME]);
    return `Started the "${SERVICE_NAME}" service. Workflows live in %ProgramData%\\Warewolf\\Resources.`;
  }
  if (process.getuid?.() !== 0) throw new Error("install-service creates a system user and a systemd unit: run it with sudo.");
  if (spawnSync("id", ["-u", SERVICE_USER]).status !== 0) {
    run("useradd", ["--system", "--home-dir", LINUX_DATA_DIR, "--create-home", "--shell", "/usr/sbin/nologin", SERVICE_USER]);
  }
  mkdirSync(LINUX_DATA_DIR, { recursive: true });
  run("chown", ["-R", `${SERVICE_USER}:${SERVICE_USER}`, LINUX_DATA_DIR]);
  chmodSync(LINUX_DATA_DIR, 0o750);
  ensureExecutable(exe);
  writeFileSync(join("/etc/systemd/system", SYSTEMD_UNIT), systemdUnit({ exe }));
  run("systemctl", ["daemon-reload"]);
  run("systemctl", ["enable", "--now", SYSTEMD_UNIT]);
  return `Started ${SYSTEMD_UNIT}. Workflows live in ${LINUX_DATA_DIR}/Resources.`;
}

function uninstallService() {
  if (process.platform === "win32") {
    spawnSync("sc.exe", ["stop", SERVICE_NAME], { stdio: "inherit" });
    run("sc.exe", ["delete", SERVICE_NAME]);
    return `Removed the "${SERVICE_NAME}" service. %ProgramData%\\Warewolf is left as it was.`;
  }
  if (process.getuid?.() !== 0) throw new Error("uninstall-service changes systemd units: run it with sudo.");
  spawnSync("systemctl", ["disable", "--now", SYSTEMD_UNIT], { stdio: "inherit" });
  rmSync(join("/etc/systemd/system", SYSTEMD_UNIT), { force: true });
  run("systemctl", ["daemon-reload"]);
  return `Removed ${SYSTEMD_UNIT}. ${LINUX_DATA_DIR} and the ${SERVICE_USER} user are left as they were.`;
}

const HELP = `warewolf-server — the Warewolf execution engine, self-hosted on 127.0.0.1:3142

  warewolf-server run [engine args]   run it in the foreground
  warewolf-server install-service     run it as the "${SERVICE_NAME}" service (sudo on Linux, Administrator on Windows)
  warewolf-server uninstall-service   remove the service, keeping its data
  warewolf-server path                print the engine executable's path

It runs without a secure.config, so anything that can reach the port can run every workflow:
keep it on loopback (WAREWOLF_SELFHOST_URLS) and don't open 3142 in a firewall.
`;

function main(argv) {
  const [command, ...rest] = argv;
  switch (command) {
    case "run": {
      const exe = enginePath();
      ensureExecutable(exe);
      const child = spawn(exe, ["--self-host", ...rest], { stdio: "inherit" });
      for (const signal of ["SIGINT", "SIGTERM"]) process.on(signal, () => child.kill(signal));
      child.on("exit", (code, signal) => process.exit(signal ? 1 : (code ?? 1)));
      return;
    }
    case "path":
      console.log(enginePath());
      return;
    case "install-service":
      console.log(installService(enginePath()));
      return;
    case "uninstall-service":
      console.log(uninstallService());
      return;
    case undefined:
    case "help":
    case "--help":
    case "-h":
      console.log(HELP);
      return;
    default:
      console.error(`Unknown command "${command}".\n\n${HELP}`);
      process.exitCode = 2;
  }
}

// Run as the bin (npm links it, so compare real paths), not when a test imports it.
if (process.argv[1] && fileURLToPath(import.meta.url) === realpathSync(process.argv[1])) {
  try {
    main(process.argv.slice(2));
  } catch (error) {
    console.error(`warewolf-server: ${error instanceof Error ? error.message : String(error)}`);
    process.exit(1);
  }
}

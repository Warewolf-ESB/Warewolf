// node --test engine/npm/cli.test.mjs
import { test } from "node:test";
import assert from "node:assert/strict";
import { join } from "node:path";
import { LINUX_DATA_DIR, SERVICE_USER, enginePath, platformPackage, systemdUnit } from "./cli.mjs";

test("each supported platform has its own package, and others are refused", () => {
  assert.equal(platformPackage("linux", "x64"), "@warewolf/engine-linux-x64");
  assert.equal(platformPackage("linux", "arm64"), "@warewolf/engine-linux-arm64");
  assert.equal(platformPackage("win32", "x64"), "@warewolf/engine-win32-x64");
  assert.throws(() => platformPackage("darwin", "arm64"), /isn't built for darwin-arm64/);
});

test("the engine is the executable in the platform package's bin folder", () => {
  const resolve = (spec) => join("/lib/node_modules", spec);
  assert.equal(enginePath("linux", "x64", resolve), join("/lib/node_modules/@warewolf/engine-linux-x64", "bin", "Warewolf.Execution.Lightweight"));
  assert.equal(enginePath("win32", "x64", resolve), join("/lib/node_modules/@warewolf/engine-win32-x64", "bin", "Warewolf.Execution.Lightweight.exe"));
  assert.throws(() => enginePath("linux", "x64", () => { throw new Error("nope"); }), /isn't installed/);
});

test("the systemd unit runs the engine self-hosted as the warewolf user, writing only its data folder", () => {
  const unit = systemdUnit({ exe: "/usr/lib/node_modules/@warewolf/engine-linux-x64/bin/Warewolf.Execution.Lightweight" });
  assert.match(unit, /^ExecStart=\S+Warewolf\.Execution\.Lightweight --self-host$/m);
  assert.match(unit, new RegExp(`^User=${SERVICE_USER}$`, "m"));
  assert.match(unit, new RegExp(`^Environment=WAREWOLF_DATA_DIR=${LINUX_DATA_DIR}$`, "m"));
  assert.match(unit, new RegExp(`^ReadWritePaths=${LINUX_DATA_DIR}$`, "m"));
  assert.match(unit, /^ProtectSystem=strict$/m);
});

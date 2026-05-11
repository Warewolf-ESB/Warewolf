#!/bin/bash
# start-engine-coverage.sh
# Starts the Warewolf lightweight engine under code coverage so the
# dotnet-isolated worker process (where the engine actually runs) is
# instrumented — not just the parent func host.
#
# Approach: static instrumentation + server-mode collector.
#   1. dotnet-coverage instrument <engine.dll> embeds tracing into the DLL.
#   2. dotnet-coverage collect --server-mode --background opens a session
#      listener that any process loading the instrumented DLL reports to.
#   3. func start runs normally; the dotnet-isolated worker loads the
#      instrumented engine and writes coverage to the session.
#
# Wrapping `func start` with `dotnet-coverage collect` (the previous
# pattern) profiles the func host only — Azure Functions launches the
# worker in a separate process whose CORECLR_* env vars are not
# inherited, so the engine is never instrumented.
#
# Usage:
#   start-engine-coverage.sh <session-id> <cov-output-xml> <bin-dir> <func-bin> [port]

set -e

SESSION_ID="${1:?session id required}"
COV_OUT="${2:?coverage output path required}"
BIN_DIR="${3:?bin dir required}"
FUNC_BIN="${4:?func binary path required}"
PORT="${5:-7071}"

COV_TOOL="${DOTNET_COVERAGE:-$HOME/.dotnet/tools/dotnet-coverage}"
[ -x "$COV_TOOL" ] || COV_TOOL="dotnet-coverage"

ENGINE_DLL="$BIN_DIR/Warewolf.Execution.Lightweight.dll"
[ -f "$ENGINE_DLL" ] || { echo "ERROR: engine DLL not found at $ENGINE_DLL"; exit 1; }

mkdir -p "$(dirname "$COV_OUT")"

echo "== Instrumenting $ENGINE_DLL (session=$SESSION_ID) =="
"$COV_TOOL" instrument "$ENGINE_DLL" --session-id "$SESSION_ID" --nologo

echo "== Starting collector in server-mode (output=$COV_OUT) =="
"$COV_TOOL" collect \
  --session-id "$SESSION_ID" \
  --server-mode \
  --background \
  --output "$COV_OUT" \
  --output-format cobertura \
  --nologo

echo "== Starting engine =="
chmod -R +x "$(dirname "$FUNC_BIN")" 2>/dev/null || true
cd "$BIN_DIR"
nohup "$FUNC_BIN" start --port "$PORT" > /tmp/func-engine.log 2>&1 &
ENGINE_PID=$!
echo "$ENGINE_PID" > "$BIN_DIR/.engine.pid"
echo "engine PID=$ENGINE_PID"

echo "== Waiting for engine ready =="
for i in $(seq 1 60); do
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 "http://localhost:${PORT}/" 2>/dev/null || echo 0)
  if [ "$STATUS" -ge 100 ] 2>/dev/null && [ "$STATUS" != "503" ]; then
    echo "Engine ready (HTTP $STATUS) after ${i}*2s"
    exit 0
  fi
  echo "[$i/60] not ready (HTTP ${STATUS:-none}); waiting 2s..."
  sleep 2
done

echo "ERROR: engine did not become ready within 120s"
echo "--- last 60 lines of /tmp/func-engine.log ---"
tail -n 60 /tmp/func-engine.log 2>/dev/null || true
exit 1

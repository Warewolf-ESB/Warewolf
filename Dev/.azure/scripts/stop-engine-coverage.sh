#!/bin/bash
# stop-engine-coverage.sh
# Counterpart to start-engine-coverage.sh. Closes the coverage session
# (which flushes the cobertura output), kills the engine + func host
# processes, and writes a placeholder coverage file if none was produced
# so downstream merge/report steps don't choke.
#
# Usage:
#   stop-engine-coverage.sh <session-id> <cov-output-xml>

SESSION_ID="${1:?session id required}"
COV_OUT="${2:?coverage output path required}"

COV_TOOL="${DOTNET_COVERAGE:-$HOME/.dotnet/tools/dotnet-coverage}"
[ -x "$COV_TOOL" ] || COV_TOOL="dotnet-coverage"

echo "== Shutting down coverage session $SESSION_ID =="
if "$COV_TOOL" shutdown "$SESSION_ID" 2>/dev/null; then
  echo "shutdown acknowledged"
else
  echo "WARNING: shutdown returned non-zero (session may already be closed)"
fi

echo "== Killing engine + func processes =="
pkill -TERM -f "azure-functions-core-tools" 2>/dev/null || true
pkill -TERM -f "func start"                  2>/dev/null || true
pkill -TERM -f "Warewolf.Execution.Lightweight" 2>/dev/null || true

echo "== Waiting for coverage file to land =="
for i in $(seq 1 60); do
  if [ -f "$COV_OUT" ]; then
    echo "Coverage file appeared after ${i}s"
    break
  fi
  sleep 1
done

if [ -f "$COV_OUT" ]; then
  kb=$(du -k "$COV_OUT" | cut -f1)
  echo "Coverage written: ${kb}KB -> $COV_OUT"
else
  echo "##[warning]Coverage file not produced: $COV_OUT (writing placeholder)"
  mkdir -p "$(dirname "$COV_OUT")"
  cat > "$COV_OUT" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" version="1.9" timestamp="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0"><sources/><packages/></coverage>
EOF
fi
exit 0

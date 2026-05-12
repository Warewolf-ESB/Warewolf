#!/bin/bash
# Proof-of-concept: pre-instrument the engine DLL, run a server-mode collector,
# launch func start, hit a workflow, shutdown, dump coverage line-rate for the
# engine classes. If line-rate>0 we've fixed the OOP-worker coverage problem.
set -e
SID=poc-engine-cov

echo '== 1. Copy /server to writable /server-rw =='
mkdir -p /server-rw
cp -a /server/. /server-rw/
ls /server-rw/Warewolf.Execution.Lightweight.dll

echo '== 2. Instrument the engine DLL =='
/opt/dotnet-tools/dotnet-coverage instrument /server-rw/Warewolf.Execution.Lightweight.dll \
  --session-id $SID \
  --nologo
echo "instrument exit=$?"

echo '== 3. Start collector in server-mode background =='
/opt/dotnet-tools/dotnet-coverage collect \
  --session-id $SID \
  --server-mode \
  --background \
  --output /tmp/engine.cobertura.xml \
  --output-format cobertura \
  --nologo
echo "background collector launched"

echo '== 4. Start engine =='
export AZURE_KEYVAULT_NAME=''
export SkipFailureToRetrieveSecret='true'
export ENABLECONSOLELOGGING='false'
export ENABLEELASTICSEARCHLOGGING='false'
export FUNCTIONS_WORKER_RUNTIME='dotnet-isolated'
export WAREWOLF_GENERATE_CI_CONFIG=1
cd /server-rw
func start --port 7071 >/tmp/func.log 2>&1 &
FUNC_PID=$!
echo "func PID=$FUNC_PID"

echo '== 5. Wait for engine =='
for i in $(seq 1 60); do
  STATUS=$(curl -s -o /dev/null -w '%{http_code}' --max-time 5 http://localhost:7071/ 2>/dev/null || echo 0)
  if [ "$STATUS" -ge 100 ] 2>/dev/null && [ "$STATUS" != '503' ]; then
    echo "Engine ready (HTTP $STATUS) after ${i}*2s"; break
  fi
  sleep 2
done
[ "$i" = 60 ] && { echo "ERROR: engine did not start"; tail -50 /tmp/func.log; exit 1; }

echo '== 6. Hit the engine to drive some code paths =='
for path in "/public/apis.json" "/secure/apis.json" "/services/apis.json"; do
  CODE=$(curl -s -o /dev/null -w '%{http_code}' http://localhost:7071$path)
  echo "  GET $path -> $CODE"
done

echo '== 7. Shutdown coverage session =='
kill -TERM $FUNC_PID 2>/dev/null || true
sleep 3
/opt/dotnet-tools/dotnet-coverage shutdown $SID 2>&1 || true
sleep 2

echo '== 8. Verify coverage file =='
if [ ! -f /tmp/engine.cobertura.xml ]; then echo "NO COVERAGE FILE"; exit 1; fi
ls -la /tmp/engine.cobertura.xml
echo '-- Engine class line-rates: --'
grep -oE 'class line-rate="[^"]+"[^>]*name="Warewolf\.Execution\.Lightweight\.[^"]+"' /tmp/engine.cobertura.xml | head -25
echo
echo '-- Count of engine classes with line-rate>0: --'
grep -oE 'class line-rate="[0-9.]+"[^>]*name="Warewolf\.Execution\.Lightweight\.[^"]+"' /tmp/engine.cobertura.xml | grep -cv 'line-rate="0"'

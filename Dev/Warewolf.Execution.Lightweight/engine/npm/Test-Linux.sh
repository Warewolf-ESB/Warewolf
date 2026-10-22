#!/bin/bash
# Installs the packed engine npm packages in a throwaway Debian container and walks the install:
# install-service (systemctl stubbed: the container has no systemd), the engine as the warewolf user,
# and its endpoints. With the WolfClaw tarball mounted at /wolfclaw it also points WolfClaw at it.
#
#   .\Build-NpmPackages.ps1 -Runtimes linux-x64 -Pack
#   docker run --rm -v "$PWD:/npm" [-v <warewolf-devops-mcp>/build/wolfclaw:/wolfclaw] node:22-bookworm bash /npm/Test-Linux.sh
#   (from Git Bash on Windows, prefix MSYS_NO_PATHCONV=1)
set -e
cd /npm/out
npm install -g ./warewolf-engine-linux-x64-*.tgz ./warewolf-engine-[0-9]*.tgz >/dev/null 2>&1
echo "== engine path"; warewolf-server path
printf '#!/bin/sh\necho "systemctl $*" >> /tmp/systemctl.log\n' > /usr/local/bin/systemctl
chmod +x /usr/local/bin/systemctl
echo "== install-service"; warewolf-server install-service
cat /tmp/systemctl.log
echo "== unit"; grep -E "ExecStart|User=|ReadWritePaths|WAREWOLF_DATA_DIR" /etc/systemd/system/warewolf-server.service
echo "== perms"; stat -c '%U:%G %a %n' /var/lib/warewolf "$(warewolf-server path)"
EXE="$(warewolf-server path)"
su -s /bin/sh warewolf -c "cd $(dirname "$EXE") && HOME=/var/lib/warewolf WAREWOLF_DATA_DIR=/var/lib/warewolf $EXE --self-host" > /tmp/engine.log 2>&1 &
for i in $(seq 1 60); do curl -s -m 1 -o /dev/null http://127.0.0.1:3142/IsLicensed && break; sleep 1; done
echo "== list_workflows"; curl -s -X POST http://127.0.0.1:3142/mcp-api/list_workflows -H "content-type: application/json" -d "{}"; echo
echo "== data"; ls -la /var/lib/warewolf/Resources | tail -n +2
if ls /wolfclaw/*.tgz >/dev/null 2>&1; then
  npm install -g /wolfclaw/*.tgz >/dev/null 2>&1
  wolfclaw init --owner telegram:42 --skip-claude >/dev/null
  wolfclaw serve > /tmp/wolfclaw.log 2>&1 &
  for i in $(seq 1 40); do curl -s -m 1 -o /dev/null http://127.0.0.1:3000/health && break; sleep 0.5; done
  echo "== wolfclaw doctor"; wolfclaw doctor || true
fi
echo "== engine errors"; grep -ciE "fatal|unhandled exception" /tmp/engine.log || true

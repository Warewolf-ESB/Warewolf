# @warewolf/engine

The Warewolf execution engine, self-contained (no .NET install needed), running on `127.0.0.1:3142`
on Linux (x64, arm64) and Windows (x64). npm installs only the engine for your platform.

```sh
npm install -g @warewolf/engine
sudo warewolf-server install-service     # Linux: the warewolf-server systemd unit
warewolf-server install-service          # Windows, elevated: the "Warewolf Server" service
```

Workflows live in `/var/lib/warewolf/Resources` on Linux and `%ProgramData%\Warewolf\Resources` on
Windows. `warewolf-server run` runs it in the foreground instead.

A locally hosted engine is free: it doesn't check for a licence.

**Keep it on loopback.** It runs without a secure.config, so anything that can reach port 3142 can run
every workflow. It listens on `127.0.0.1` unless you set `WAREWOLF_SELFHOST_URLS`; don't open the port
in a firewall.

On Linux it needs `libicu` (`apt install libicu72`, `dnf install libicu`).

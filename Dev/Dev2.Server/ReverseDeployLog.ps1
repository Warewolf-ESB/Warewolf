sc.exe stop "Warewolf Server"
Wait-Process -Name "Warewolf Server" -ErrorAction SilentlyContinue
Move-Item "C:\programdata\warewolf\Server Log\warewolf-server.log" "$PSScriptRoot\TestResults\warewolf-server.log"

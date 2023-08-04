echo Adding Warewolf administrator user from environment variables an tarting Warewolf server as %SERVER_USERNAME%
echo 1. Create Warewolf Administrators group.
NET localgroup "Warewolf Administrators" /ADD
echo 2. Create new Warewolf Administrator.
NET user "%SERVER_USERNAME%" "%SERVER_PASSWORD%" /ADD /Y
if (%ERRORLEVEL% == 2)(exit 1)
NET user "%SERVER_USERNAME%" "%SERVER_PASSWORD%" /Y
if (%ERRORLEVEL% == 2)(exit 1)
echo 3. Add new Warewolf Administrator to Administrators group.
NET localgroup "Administrators" "%SERVER_USERNAME%" /ADD
echo 4. Add new Warewolf Administrator to Warewolf Administrators group.
NET localgroup "Warewolf Administrators" "%SERVER_USERNAME%" /ADD
echo 5. Start Warewolf server.
sc create "Warewolf Server" binPath= "\"C:\Program Files\dotnet\dotnet.exe\" \"C:\server\Warewolf Server.dll\""
sc start "Warewolf Server"
powershell -Command Get-Content "C:\programdata\Warewolf\Server` Log\warewolf-server.log" -Wait
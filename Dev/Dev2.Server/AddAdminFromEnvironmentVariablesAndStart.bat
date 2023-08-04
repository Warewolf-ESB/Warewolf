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
echo 5. Remove environment variables.
REG delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /F /V SERVER_USERNAME
REG delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /F /V SERVER_PASSWORD
echo 6. Start Warewolf server.
"C:\Program Files\dotnet\dotnet.exe" "%~dp0Warewolf Server.dll" --interactive
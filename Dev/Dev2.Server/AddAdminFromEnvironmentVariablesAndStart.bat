echo Adding Warewolf administrator user from environment variables an tarting Warewolf server as %SERVER_USERNAME%
		Write-Host 1. Create Warewolf Administrators group.
		NET localgroup "Warewolf Administrators" /ADD
		Write-Host 2. Create new Warewolf Administrator.
NET user "%SERVER_USERNAME%" "%SERVER_PASSWORD%" /ADD /Y
if (%ERRORLEVEL% == 2)(exit 1)
NET user "%SERVER_USERNAME%" "%SERVER_PASSWORD%" /Y
if (%ERRORLEVEL% == 2)(exit 1)
echo 3. Add new Warewolf Administrator to Administrators group.
NET localgroup "Administrators" "%SERVER_USERNAME%" /ADD
echo 4. Add new Warewolf Administrator to Warewolf Administrators group.
NET localgroup "Warewolf Administrators" "%SERVER_USERNAME%" /ADD
"%`DP0\Warewolf Server.exe"

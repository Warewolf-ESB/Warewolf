REM ===== START =====
set verPath="C:\TfsBuildUtils\Versioning\verpatch"

REM === PROCESS SERVER ===
%verPath% "D:\Automated Builds\NightlyBuild\Obfuscated_Artifacts\Warewolf Server.exe" %1

REM === PROCESS STUDIO ===
%verPath% "D:\Automated Builds\NightlyBuild\Obfuscated_Artifacts\Warewolf Studio.exe" %1

REM === PROCESS SQL DLL ==
%verPath% "D:\Automated Builds\NightlyBuild\Obfuscated_Artifacts\Warewolf.Sql.dll" %1

REM ==== PROCESS DLLS ====
for %%f in ("D:\Automated Builds\NightlyBuild\Obfuscated_Artifacts\Dev2*.dll") do %verPath% "%%f" %1
exit /b

Warewolf.Sq
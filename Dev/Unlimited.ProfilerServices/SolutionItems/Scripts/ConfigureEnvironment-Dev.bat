set "params=%*"
cd /d "%~dp0" && ( if exist "%temp%\getadmin.vbs" del "%temp%\getadmin.vbs" ) && fsutil dirty query %systemdrive% 1>nul 2>nul || (  echo Set UAC = CreateObject^("Shell.Application"^) : UAC.ShellExecute "cmd.exe", "/k cd ""%~sdp0"" && %~s0 %params%", "", "runas", 1 >> "%temp%\getadmin.vbs" && "%temp%\getadmin.vbs" && exit /B )

powershell -file GenerateSybolicLink.ps1 ..\..\Warewolf.ProfilerServices\Profiler ..\..\Warewolf.ProfilerServices\Sources

powershell -file ConfigureFiles.ps1 ..\..\Warewolf.ProfilerServices\SOURCES ..\..\Warewolf.ProfilerServices\SOURCES Develop,Default True -OverwriteTarget

EXIT
@echo off
IF EXIST %~dp0Warewolf.Launcher.exe (
  %~dp0Warewolf.Launcher.exe --AdminMode
) ELSE (
  ECHO Warewolf.Launcher.exe not found. Compile Warewolf.Launcher.csproj and run RunTests.bat from the build output ^(bin^) directory.
  pause
)
@echo on
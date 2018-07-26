@echo off
IF EXIST "%~dp0Warewolf.Launcher.exe" (
  "%~dp0Warewolf.Launcher.exe" --AdminMode %1 %2 %3 %4 %5 %6 %7 %8 %9
) ELSE (
  IF EXIST "%~dp0bin\Debug\Warewolf.Launcher.exe" (
    "%~dp0bin\Debug\Warewolf.Launcher.exe" --AdminMode %1 %2 %3 %4 %5 %6 %7 %8 %9
  ) ELSE (
    ECHO Warewolf.Launcher.exe not found. Compile Warewolf.Launcher.csproj.
    pause
  )
)
@echo on
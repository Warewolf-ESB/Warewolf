powershell -ExecutionPolicy Bypass -File %~dp0..\Compile.ps1 -UITesting -Server -Studio -SolutionWideOutputs -SkipCodeAnalysis %1 %2 %3 %4 %5
pause
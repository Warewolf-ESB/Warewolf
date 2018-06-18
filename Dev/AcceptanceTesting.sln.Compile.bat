cd %~dp0..\
powershell -ExecutionPolicy Bypass -File %~dp0Compile.ps1 --AcceptanceTesting -SolutionWideBuildOutputs %1 %2 %3 %4 %5
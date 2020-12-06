powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\..\..\..\Compile.ps1" -AcceptanceTesting -SolutionWideOutputs
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -Projects Dev2.Infrastructure.Tests -Coverage -RunInDocker
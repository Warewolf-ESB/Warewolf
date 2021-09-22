mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryCount 6 -RetryRebuild -Projects Dev2.Studio.Core.Tests -Category "Intellisense Textbox Visual Tree" -Coverage
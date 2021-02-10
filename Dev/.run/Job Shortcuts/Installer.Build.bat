echo off
if not exist "%~dp0installer" ( git -C "%~dp0." clone https://gitlab.com/warewolf/installer )
if not exist "%~dp0installer" ( echo Installer git clone failed. Try cloning, copying or otherwise creating directory at "%~dp0installer" containing https://gitlab.com/warewolf/installer with this command: git -C "%~dp0." clone https://gitlab.com/warewolf/installer )
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\..\..\Compile.ps1" -AcceptanceTesting -ProjectSpecificOutputs -Config Release -AutoVersion
xcopy "%~dp0..\..\Dev2.Server\bin\Release\net48\win" "%~dp0installer\WarewolfInstaller\ProductBuild\Server\" /S /Y
xcopy "%~dp0..\..\Dev2.Studio\bin\Release\net48\win" "%~dp0installer\WarewolfInstaller\ProductBuild\Studio\" /S /Y
"%~dp0installer\Build.bat"
echo New installer available at "%~dp0installer\WixWPF\bin\Debug\Warewolf-0.0.0.0.exe"
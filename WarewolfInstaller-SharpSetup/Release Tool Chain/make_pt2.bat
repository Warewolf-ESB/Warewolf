
@echo --Start Release Process-- 

@echo "Using Release %1%"
@echo "Make sure you have updated the build meta data for the installer project"

@echo -Sign DLLS-
REM WarewolfCodeSign - sn.exe ContainerName
REM "C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\*.dll"
REM "C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\*.dll"

@echo -Staging For Build-
rd /S /Q "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"
rd /S /Q "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"

mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"
mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"

xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"
xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"

xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"
xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"

@echo -Preping Example Workflows-
mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services"
mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Sources"

PrepForShip.exe "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services" "C:\Development\Release %1%\BPM Resources - Release\Services"
copy "C:\Development\Release %1%\BPM Resources - Release\Sources\*.*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Sources\"

@echo -Make Installer-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\WarewolfInstaller-SharpSetup\WarewolfInstaller-SharpSetup.sln" /build

@echo --End Release Process--
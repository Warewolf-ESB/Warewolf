
cls
@echo --Start Release Process Pt 2-- 

@echo "Using Release %1% AND %2%"
@echo "Make sure you have updated the build meta data for the installer project"

@echo -Sign DLLS-
"C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\*.dll"
"C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\*.dll"

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

cd "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain"
PrepForShip.exe "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services" "C:\Development\Release %1%\BPM Resources - Release\Services"
cd "C:\Development\Release %1%\BPM Resources - Release\Sources\"
xcopy /S /Y "*.*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Sources\"

@echo -Versioning Artifacts-
xcopy /X /Y "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\verpatch-bin-1.0.10\*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"
xcopy /X /Y "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\verpatch-bin-1.0.10\*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"

cd "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"
updatever-server.bat "%2%"

cd "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"
updatever-studio.bat "%2%"

@echo --End Release Process--
REM 26.11.2013 - Basic Release Build Script ;)

@echo --Start Release Process-- 

@echo "Make sure your source is update to date for Release %1%"
@echo "Using %2% as the release number"
@echo "Make sure you have updated the build meta data for the installer project"

@set /P waitKey=Press Any Key When Ready

@echo "Using Branch %1% for build"

@echo -Build Server-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Server.sln" /clean
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Server.sln" /build

@echo -Build Studio-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Studio.sln" /clean
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Studio.sln" /build

@echo -Staging For Obfuscation-
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\"
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"
xcopy /S /Y "C:\Development\Release %1%\Dev2.Server\bin\Debug\" "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"

@echo -Obfuscating Build-
wget http://RSAKLFSVRTFSBLD:1234/services/Obfuscate Artifacts

@echo -Staging For Build-
rd /S /Q "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"
rd /S /Q "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"

xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"
xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"

xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"
xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"

@echo -Versioning Artifacts-
xcopy /X /Y "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\verpatch-bin-1.0.10\*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"
xcopy /X /Y "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\verpatch-bin-1.0.10\*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"

"C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\updatever-server.bat %2%"
"C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\updatever-studio.bat %2%"

@echo -Preping Example Workflows-
mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services"

PrepForShip.exe "C:\Development\Release %1%\BPM Resources - Release\Services" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services"
xcopy /S /Y "C:\Development\Release %1%\BPM Resources - Release\Sources" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"

@echo -Make Installer-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\WarewolfInstaller-SharpSetup\WarewolfInstaller-SharpSetup.sln" /build

@echo --End Release Process--

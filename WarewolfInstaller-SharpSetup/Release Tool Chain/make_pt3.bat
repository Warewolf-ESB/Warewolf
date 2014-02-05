cls

@echo "Making Release %1%"
@echo -Make Installer-
REM "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\WarewolfInstaller-SharpSetup\WarewolfInstaller-SharpSetup.sln" /build

@echo -Sign Installer-
REM delete "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf-%1%.exe"
REM move "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf_Installer.exe" "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf-%1%.exe"
"C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf_Installer.exe"

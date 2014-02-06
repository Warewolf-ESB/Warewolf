REM 26.11.2013 : V1.0 - Basic Release Build Script ;)
REM 08.01.2014 : V1.1 - Added Code Signing ;)
REM 06.02.2014 : v1.2 - Amended to run as a single file with proper dll versioning

cls
REM SET COLOR
COLOR 97

@echo --Start Release Process-- 

IF "%1%"=="" GOTO EXIT
IF "%2%"=="" GOTO EXIT
cls

@echo "Make sure your source is update to date for Release %1%"
@echo "Using %2% as the release number"
@echo "Make sure you have updated the build meta data for the installer project"

@set /P waitKey=Press Enter When Ready or Q/q to exit 

IF %waitKey%=="" GOTO BUILD
IF %waitKey%=="q" GOTO EXIT_NICE
IF %waitKey%=="Q" GOTO EXIT_NICE

:BUILD
cls 

@echo "Using Branch %1% for build"

@echo -Update Source-
cd "C:\Development\Release %1%"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\tf.exe" get

@echo -Build Server-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Server.sln" /clean
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Server.sln" /build release

@echo -Build Studio-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Studio.sln" /clean
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\Release %1%\Studio.sln" /build release

@echo -Staging For Obfuscation-
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\"
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"

mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\"
mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"
mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Webs"
mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Webs\wwwroot"

@set /P waitKey=Press Enter When Ready 

xcopy "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Static_Artifacts\Warewolf.snk" "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"
rd /S /Q "C:\Development\Release %1%\Dev2.Server\bin\Release\Workspaces\"
REM Copy Webs - silly debug directory
xcopy /S /Y "C:\Development\Release %1%\Dev2.Server\bin\Debug\Webs\wwwroot" "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Webs\wwwroot\"
xcopy /S /Y "C:\Development\Release %1%\Dev2.Server\bin\Release\*" "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"
xcopy /S /Y "C:\Development\Release %1%\Dev2.Studio\bin\Release\*" "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"

rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Sources"
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Services"
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Studio Server"
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\themes"
rd /S /Q "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Plugins"

@echo -Obfuscating Build-
start "~\iexplore.exe" "http://RSAKLFSVRTFSBLD:3142/services/Obfuscate Artifacts"
echo Service must run as IntegrationTester - And Must Support non NTLM request ;)
@set /P waitKey=Press Enter When Ready To Move Beyond Obfuscation

cls
@echo --Start Release Process Pt 2-- 

@echo "Using Release Branch %1% AND Version %2%"
@echo "Make sure you have updated the build meta data for the installer project"

@echo -Sign DLLS-
"C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\*.dll"
"C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\*.dll"

@echo -Staging For Build-
REM rd /S /Q "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"
REM rd /S /Q "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"

REM mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\"
REM mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio\"

xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"
xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"

xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"
xcopy /S /Y "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"

@echo -Preping Example Workflows-
REM mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services"
REM mkdir "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Sources"

REM cd "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain"
REM PrepForShip.exe "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Services" "C:\Development\Release %1%\BPM Resources - Release\Services"
REM cd "C:\Development\Release %1%\BPM Resources - Release\Sources\"
REM xcopy /S /Y "*.*" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server\Sources\"

@echo -Versioning Artifacts-
xcopy /X /Y "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\verpatch-bin-1.0.10\verpatch.exe" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"
xcopy /X /Y "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\verpatch-bin-1.0.10\verpatch.exe" "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"

@echo -Version Server-
cd "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Server"
echo verpatch "Warewolf Server.exe" "%2%" > version.bat
echo for %%f in (Dev2*.dll) do verpatch %%f "%2%" >> version.bat
version.bat

@echo -- Version Studio --
cd "C:\Development\WarewolfInstaller-SharpSetup\ProductBuild\Studio"
echo verpatch "Warewolf Server.exe" "%2%" > version.bat
echo for %%f in (Dev2*.dll) do verpatch %%f "%2%" >> version.bat
version.bat

@echo "Making Release %2%"
@echo -Make Installer-
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" "C:\Development\WarewolfInstaller-SharpSetup\WarewolfInstaller-SharpSetup.sln" /build release

@echo -Sign Installer-
delete "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf-%2%.exe"
move "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf_Installer.exe" "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf-%2%.exe"
"C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe" sign /f "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Warewolf Cert.pfx" /p 5678 /ac "C:\Development\WarewolfInstaller-SharpSetup\Release Tool Chain\Comodo_CA.cer" "C:\Development\WarewolfInstaller-SharpSetup\Debug\Warewolf_Installer.exe"

@echo ******End Release Process******
goto EOF

:EXIT
	cls
	COLOR 47
	cls
	@echo You need to pass the release branch as first parameter
	@echo And you need to pass the release number as the second parameter
	@echo ie make_full.bat 0.4.1.x 0.4.1.1
	goto EOF
:EXIT_NICE
	cls
	COLOR 08
	cls
	@echo *****Aborted release build*****
:EOF

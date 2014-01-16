REM 26.11.2013 : V1.0 - Basic Release Build Script ;)
REM 08.01.2014 : V1.1 - Added Code Signing ;)

cls
REM SET OOLOR
COLOR 97

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

mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Artifacts\"
mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\"
mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Webs"
mkdir "\\RSAKLFSVRTFSBLD\Automated Builds\NightlyBuild\Obfuscated_Staging\Webs\wwwroot"


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
REM - Service must run as IntegrationTester - And Must Support non NTLM request ;)
REM wget "http://RSAKLFSVRTFSBLD:3142/services/Obfuscate Artifacts"


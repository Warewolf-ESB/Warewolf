REM Must be placed into the Integration Run Enviroment in C:\TfsBuildUtils\run.bat
REM Setup the files required to run integration test ;)

taskkill /F /IM "Warewolf Server.exe" /T
rmdir /S /Q "C:\IntegationRun\Merge"
mkdir "C:\IntegationRun\Merge"

xcopy /Q /E /Y "\\rsaklfsvrtfsbld\Automated Builds\DevMergeStaging" "C:\IntegationRun\Merge"
start "" /B "C:\IntegationRun\Merge\Warewolf Server.exe"
timeout 30

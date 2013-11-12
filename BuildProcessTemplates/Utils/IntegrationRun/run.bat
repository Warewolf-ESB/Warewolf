REM == START ENV ==

taskkill /F /IM "Warewolf Server.exe" /T
rmdir /S /Q C:\IntegrationRun\Merge
mkdir C:\IntegrationRun\Merge
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\DevMergeStaging" "C:\IntegrationRun\Merge"
start "" /B "C:\IntegrationRun\Merge\Warewolf Server.exe"
timeout 8
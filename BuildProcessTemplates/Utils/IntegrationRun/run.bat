REM == START ENV ==

taskkill /F /IM "Warewolf Server.exe" /T
rmdir /S /Q C:\IntegrationRun\Binaries
mkdir C:\IntegrationRun\Binaries
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\DevMergeStaging" "C:\IntegrationRun\Binaries"
start "" /B "C:\IntegrationRun\Binaries\Warewolf Server.exe"
timeout 8
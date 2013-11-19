REM == Pull specific changeset build ==

rmdir /S /Q C:\IntegrationRun\Merge
mkdir C:\IntegrationRun\Merge
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\TestRunStaging\%1" "C:\IntegrationRun\Merge"
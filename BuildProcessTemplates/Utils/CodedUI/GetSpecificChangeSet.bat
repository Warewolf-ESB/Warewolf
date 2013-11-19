REM == Pull specific changeset build ==

rmdir /S /Q C:\CodedUI\Merge
mkdir C:\CodedUI\Merge
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\TestRunStaging\%1" "C:\CodedUI\Merge"
timeout 8
REM == START ENV ==

taskkill /F /IM "Warewolf Server.exe"
taskkill /F /IM "Warewolf Studio.exe"
rmdir /S /Q C:\CodedUI\Merge
mkdir C:\CodedUI\Merge
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\ReleaseStaging" "C:\CodedUI\Merge"
start "" /B "C:\CodedUI\Merge\Warewolf Server.exe"
timeout 15
start "" /B "C:\CodedUI\Merge\Warewolf Studio.exe"
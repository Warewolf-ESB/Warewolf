REM == START ENV ==

rmdir /S /Q C:\CodedUI\Merge
mkdir C:\CodedUI\Merge
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\DevMergeStaging" "C:\CodedUI\Merge"
start "" /B "C:\CodedUI\Merge\Warewolf Server.exe"
timeout 10
start "" /B "C:\CodedUI\Merge\Warewolf Studio.exe"
REM timeout 60
REM ping -n 10000 127.0.0.1
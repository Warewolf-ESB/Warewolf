
rmdir /S /Q C:\CodedUI\Merge
mkdir C:\CodedUI\Merge
xcopy /Q /E "\\RSAKLFSVRTFSBLD\Automated Builds\GatedStaging" "C:\CodedUI\Merge"
"C:\CodedUI\Merge\Warewolf Server.exe"
timeout 15
"C:\CodedUI\Merge\Warewolf Studio.exe"
ping -n 10000 127.0.0.1
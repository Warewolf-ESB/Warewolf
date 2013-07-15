
rmdir /S /Q C:\CodedUI
mkdir C:\CodedUI
xcopy /Q /E "\\RSAKLFSVRTFSBLD\Automated Builds\GatedStaging\*.*" "C:\CodedUI"
"C:\CodedUI\Warewolf Server.exe"
REM "C:\CodedUI\Warewolf Studio.exe"
ping -n 10000 127.0.0.1
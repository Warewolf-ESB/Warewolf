
rmdir /S /Q C:\CodedUI
mkdir C:\CodedUI
xcopy /Q /E "\\RSAKLFSVRTFSBLD\Automated Builds\GatedStaging\*.*" "C:\CodedUI"
"C:\CodedUI\Warewolf Server.exe"
"C:\CodedUI\Warewolf Studio.exe"
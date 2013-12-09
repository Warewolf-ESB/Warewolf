REM == START ENV ==

taskkill /F /IM "Warewolf Server.exe" /T
rmdir /S /Q %1
mkdir %1
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\DEVCodedUIStaging" "%1"
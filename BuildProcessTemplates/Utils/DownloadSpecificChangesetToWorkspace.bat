mkdir "C:\Builds\TestRunWorkspace\Binaries"
del /Q "C:\Builds\TestRunWorkspace\Binaries\*.*"
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\TestRunStaging\%1" "C:\Builds\TestRunWorkspace\Binaries"
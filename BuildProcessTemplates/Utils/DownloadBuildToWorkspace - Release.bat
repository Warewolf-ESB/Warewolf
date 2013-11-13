mkdir "C:\Builds\TestRunWorkspace\Binaries"
del /Q "C:\Builds\TestRunWorkspace\Binaries\*.*"
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\ReleaseStaging" "C:\Builds\TestRunWorkspace\Binaries"
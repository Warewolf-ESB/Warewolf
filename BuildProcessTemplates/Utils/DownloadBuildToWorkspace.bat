rmdir /S /Q C:\Builds\TestRunWorkspace\Binaries
mkdir C:\Builds\TestRunWorkspace\Binaries
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\DevMergeStaging" "C:\Builds\TestRunWorkspace\Binaries"
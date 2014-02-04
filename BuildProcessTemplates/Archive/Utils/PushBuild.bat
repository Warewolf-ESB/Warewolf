rmdir /S /Q C:\Builds\TestRunWorkspace\Binaries
mkdir C:\Builds\TestRunWorkspace\Binaries
xcopy /Q /E /Y "C:\Builds\BuildWorkspace\Binaries" "C:\Builds\TestRunWorkspace\Binaries"
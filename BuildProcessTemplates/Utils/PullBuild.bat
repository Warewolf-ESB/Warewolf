rmdir /S /Q C:\Builds\AllTestRuns
mkdir C:\Builds\AllTestRuns
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\DevMergeStaging" "C:\Builds\AllTestRuns"
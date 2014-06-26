for /f "tokens=*" %%A in ('dir "\\rsaklfsvrtfsbld\AutomatedBuilds\DevMergeStaging\Passed" /AD /O-D /B') do (set recent=%%A& goto exit)
:exit
CD "%~dp0"
robocopy "\\rsaklfsvrtfsbld\AutomatedBuilds\DevMergeStaging\Passed\%recent%" "%CD%" *.* /s
pause
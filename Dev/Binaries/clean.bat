REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\DevMergeStaging" %%x in (*) do rd /s /q "%%x"
exit /b
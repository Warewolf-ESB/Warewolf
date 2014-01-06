REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\UITestResults%1" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\UITestResults%1\*.*"
del /Q "C:\Builds\TestRunWorkspace\UITestResults%1\*.*"
exit /b
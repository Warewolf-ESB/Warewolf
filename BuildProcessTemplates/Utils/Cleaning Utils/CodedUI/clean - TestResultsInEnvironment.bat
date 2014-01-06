REM  Clean Staging Directory

@echo off
@echo Cleaning UITestResults%1 ...
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\UITestResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\UITestResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\UITestResults\*.*"
exit /b
REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\AcceptanceTestResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\AcceptanceTestResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\AcceptanceTestResults\*.*"
exit /b
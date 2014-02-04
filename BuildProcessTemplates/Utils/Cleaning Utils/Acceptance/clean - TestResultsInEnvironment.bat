REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\AcceptanceTestingResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\AcceptanceTestingResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\AcceptanceTestingResults\*.*"
exit /b
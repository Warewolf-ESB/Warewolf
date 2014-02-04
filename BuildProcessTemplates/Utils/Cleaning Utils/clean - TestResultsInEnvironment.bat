REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\TestResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\TestResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\TestResults\*.*"
exit /b
REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Build\TestRunWorkspace\UITestResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Build\TestRunWorkspace\UITestResults\*.*"
del /Q "C:\Build\TestRunWorkspace\UITestResults\*.*"
exit /b
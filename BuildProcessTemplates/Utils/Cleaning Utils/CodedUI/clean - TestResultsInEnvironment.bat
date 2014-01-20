REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\CodedUITestResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\CodedUITestResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\CodedUITestResults\*.*"
exit /b
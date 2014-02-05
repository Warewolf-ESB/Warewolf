REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\Unit Tests" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\Unit Tests\*.*"
del /Q "C:\Builds\TestRunWorkspace\Unit Tests\*.*"
exit /b
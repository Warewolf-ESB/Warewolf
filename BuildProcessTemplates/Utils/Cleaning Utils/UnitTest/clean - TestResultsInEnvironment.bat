REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\BuildWorkspace\Unit Tests" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\BuildWorkspace\Unit Tests\*.*"
del /Q "C:\Builds\BuildWorkspace\Unit Tests\*.*"
exit /b
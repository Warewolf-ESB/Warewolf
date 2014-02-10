REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Development\Dev\Unit Tests" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Development\Dev\Unit Tests\*.*"
del /Q "C:\Development\Dev\Unit Tests\*.*"
exit /b
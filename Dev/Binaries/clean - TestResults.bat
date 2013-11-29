REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Development\Dev\TestResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Development\Dev\TestResults\*.*"
del /Q "C:\Development\Dev\TestResults\*.*"
exit /b
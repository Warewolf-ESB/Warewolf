REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Binaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Binaries\*.*"
del /Q "C:\Binaries\*.*"
exit /b
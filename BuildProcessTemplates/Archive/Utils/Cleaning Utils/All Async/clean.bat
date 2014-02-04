REM  Clean ENV Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\Binaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\Binaries\*.*"
del /Q "C:\Builds\Binaries\*.*"
exit /b
REM  Clean ENV Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\IntegrationRun\Binaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\IntegrationRun\Binaries\*.*"
del /Q "C:\IntegrationRun\Binaries\*.*"
exit /b
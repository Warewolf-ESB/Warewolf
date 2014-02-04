REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\IntegrationBinaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\IntegrationBinaries\*.*"
del /Q "C:\IntegrationBinaries\*.*"
exit /b
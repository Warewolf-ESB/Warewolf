REM  Clean Directory Passed as Param

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Users\IntegrationTester\AppData\Local\Temp" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Users\IntegrationTester\AppData\Local\Temp\*.*"
del /Q "C:\Users\IntegrationTester\AppData\Local\Temp\*.*"
exit /b
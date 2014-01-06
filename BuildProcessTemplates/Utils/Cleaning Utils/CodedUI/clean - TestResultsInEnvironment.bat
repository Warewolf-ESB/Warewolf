REM  Clean Staging Directory

@echo off
@echo Cleaning UITestResults%1 ...
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\UITestResults%1" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\UITestResults%1\*.*"
del /Q "C:\UITestResults%1\*.*"
exit /b
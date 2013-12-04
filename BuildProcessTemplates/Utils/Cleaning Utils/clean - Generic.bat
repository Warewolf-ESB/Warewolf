REM  Clean Directory Passed as Param

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "%1" %%x in (*) do rd /s /q "%%x"
attrib -R "%1\*.*"
del /Q "%1\*.*"
exit /b
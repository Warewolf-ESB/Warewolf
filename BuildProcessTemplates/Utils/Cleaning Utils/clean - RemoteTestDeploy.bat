REM  Clean Test Deployment Directory in Remote Environment

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "%localappdata%\VSEQT\QTAgent" %%x in (*) do rd /s /q "%%x"
attrib -R "%localappdata%\VSEQT\QTAgent\*.*"
del /Q "%localappdata%\VSEQT\QTAgent\*.*"
exit /b
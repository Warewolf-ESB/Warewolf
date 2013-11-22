REM  Clean ENV Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\CodedUI\Binaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\CodedUI\Binaries\*.*"
del /Q "C:\CodedUI\Binaries\*.*"
exit /b
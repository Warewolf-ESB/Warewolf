REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\CodedUIBinaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\CodedUIBinaries\*.*"
del /Q "C:\CodedUIBinaries\*.*"
exit /b
REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\CodedUITestsResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\CodedUITestsResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\CodedUITestsResults\*.*"
exit /b
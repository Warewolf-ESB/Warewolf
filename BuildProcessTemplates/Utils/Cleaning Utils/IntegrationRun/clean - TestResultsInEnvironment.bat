REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\TestRunWorkspace\IntegrationTestsResults" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\TestRunWorkspace\IntegrationTestsResults\*.*"
del /Q "C:\Builds\TestRunWorkspace\IntegrationTestsResults\*.*"
exit /b
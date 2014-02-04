REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\DEVIntegrationStaging" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\DEVIntegrationStaging\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\DEVIntegrationStaging\*.*"
exit /b
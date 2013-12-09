REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\ReleaseIntegrationStaging" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\ReleaseIntegrationStaging\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\ReleaseIntegrationStaging\*.*"
exit /b
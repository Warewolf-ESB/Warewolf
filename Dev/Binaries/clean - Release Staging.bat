REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\ReleaseStaging" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\ReleaseStaging\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\ReleaseStaging\*.*"
exit /b
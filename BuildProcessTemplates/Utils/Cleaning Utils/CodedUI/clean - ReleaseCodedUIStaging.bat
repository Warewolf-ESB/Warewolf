REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\ReleaseCodedUIStaging" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\ReleaseCodedUIStaging\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\ReleaseCodedUIStaging\*.*"
exit /b
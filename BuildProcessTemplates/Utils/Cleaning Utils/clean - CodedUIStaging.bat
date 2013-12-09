REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\DEVCodedUIStaging" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\DEVCodedUIStaging\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\DEVCodedUIStaging\*.*"
exit /b
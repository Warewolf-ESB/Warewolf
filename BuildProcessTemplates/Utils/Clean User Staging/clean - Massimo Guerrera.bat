REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Massimo Guerrera" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Massimo Guerrera\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Massimo Guerrera\*.*"
exit /b
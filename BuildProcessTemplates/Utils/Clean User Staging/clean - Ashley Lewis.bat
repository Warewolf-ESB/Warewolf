REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Ashley Lewis" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Ashley Lewis\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Ashley Lewis\*.*"
exit /b
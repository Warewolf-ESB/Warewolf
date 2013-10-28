REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Trevor Williams-Ros" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Trevor Williams-Ros\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Trevor Williams-Ros\*.*"
exit /b
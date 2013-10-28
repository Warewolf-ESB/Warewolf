REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Tshepo Ntlhokoa" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Tshepo Ntlhokoa\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\UserDevStaging\Tshepo Ntlhokoa\*.*"
exit /b
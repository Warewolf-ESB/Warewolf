REM  Clean workspace binaries directory

call :cleanDIR
goto :eof

:cleanDIR
for /d /r "C:\Builds\BuildWorkspace\Binaries" %%x in (*) do rd /s /q "%%x"
attrib -R "C:\Builds\BuildWorkspace\Binaries\*.*"
del /Q "C:\Builds\BuildWorkspace\Binaries\*.*"
exit /b
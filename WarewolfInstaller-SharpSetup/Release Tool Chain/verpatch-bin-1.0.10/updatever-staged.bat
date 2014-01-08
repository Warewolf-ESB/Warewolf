
set ver=%1%

call :updateEXE
call :updateDLL
goto :eof

:updateEXE

verpatch "Warewolf Server.exe" %ver%
verpatch "Warewolf Studio.exe" %ver%


:updateDLL
rem Do whatever you want here over the files of this subdir, for example:
for %%f in (Dev2*.dll) do verpatch %%f %ver%
exit /b

call :updateEXE
call :updateDLL
goto :eof

:updateEXE

./verpatch "Warewolf Studio.exe" %1%


:updateDLL
rem Do whatever you want here over the files of this subdir, for example:
for %%f in (Dev2*.dll) do verpatch %%f %1%
exit /b
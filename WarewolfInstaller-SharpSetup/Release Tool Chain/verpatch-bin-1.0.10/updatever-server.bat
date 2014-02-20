
set ver=%1%
verpatch "Warewolf Server.exe" "0.4.2.5"
rem Do whatever you want here over the files of this subdir, for example:
for %%f in (Dev2*.dll) do verpatch %%f "0.4.2.5"

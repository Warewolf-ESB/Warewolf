taskkill /im "Warewolf Studio.exe"
%DeploymentDirectory%\Dev2.Server.exe -x
taskkill /im "Warewolf Server.exe"
FOR /D %%p IN ("%DeploymentDirectory%\*.*") DO rmdir "%%p" /s /q
exit 0
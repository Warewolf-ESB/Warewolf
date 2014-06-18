taskkill /im "Warewolf Studio.exe"
%DeploymentDirectory%\Dev2.Server.exe -x
taskkill /im "Warewolf Server.exe"
exit 0
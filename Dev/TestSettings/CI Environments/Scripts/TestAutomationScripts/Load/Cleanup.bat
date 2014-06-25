sc stop "Warewolf Server"
exit 0
FOR /D %%p IN ("%DeploymentDirectory%\*.*") DO rmdir "%%p" /s /q
exit 0
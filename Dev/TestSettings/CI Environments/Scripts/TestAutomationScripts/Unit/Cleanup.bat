FOR /D %%p IN ("%DeploymentDirectory%\*.*") DO rmdir "%%p" /s /q
exit 0
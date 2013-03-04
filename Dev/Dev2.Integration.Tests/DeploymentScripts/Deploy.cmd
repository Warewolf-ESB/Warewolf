REM set build path
set buildlocation=%1


set svndirectory= "\\rsaklfsvrtfsbld\Automated Builds\LatestDevelopmentResources"

REM set deployment path
set targetdir="C:\Deploy"\

REM create deployment directory
if not exist %targetdir% (cmd /c mkdir %targetdir%)

REM copy build to the deployment directory
robocopy %buildlocation% %targetdir% /E
robocopy %svndirectory% %targetdir% /E

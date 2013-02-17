REM set build path
set buildlocation=%1


set svndirectory= "\\rsaklfsvrtfsbld\Automated Builds\LatestDevelopmentResources"

REM set deployment path
set targetdir="C:\Deploy"\
set executedir="C:\ExecuteFrom"

REM create deployment directory
if not exist %targetdir% (cmd /c mkdir %targetdir%)

REM create execution directory
if exist %executedir% (cmd /c rmdir /Q %executedir%)

if not exist %executedir% (cmd /c mkdir %executedir%)

REM copy build to the deployment directory
robocopy %buildlocation% %targetdir% /E
robocopy %svndirectory% %targetdir% /E

robocopy %targetdir% %executedir% /E

REM if you are using a deployment package you can run it here, after you copy it to your deployment directory

REM ** Start Server **
C:\ExecuteFrom\Dev2.Server.exe -i
timeout /T 120 /NOBREAK

C:\ExecuteFrom\Dev2.Studio.exe
timeout /T 60 /NOBREAK
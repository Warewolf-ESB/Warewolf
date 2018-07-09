FROM microsoft/windowsservercore

RUN NET localgroup "Warewolf Administrators" /ADD
EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]
RUN New-Item -Path Server -ItemType Directory
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"
ENV SERVER_PASSWORD "W@rEw0lf@dm1n"

RUN New-Item -Path "programdata\Warewolf" -ItemType Directory
RUN Copy-Item -Path \"Server\Resources - Release\*\" -Destination \"programdata\Warewolf\" -Recurse -Force
ADD NewAzureUser.bite \programdata\Warewolf\Resources

ENTRYPOINT cmd /c NET user WarewolfAdmin "$env:SERVER_PASSWORD" /ADD /Y;cmd /c NET localgroup "Administrators" WarewolfAdmin /ADD;cmd /c NET localgroup "Warewolf Administrators" WarewolfAdmin /ADD;Start-Process -FilePath "$env:SERVER_PATH" -ArgumentList "--interactive" -Verb RunAs -Wait

FROM microsoft/windowsservercore

RUN NET user WarewolfAdmin W@rEw0lf@dm1n /ADD
RUN NET localgroup "Administrators" WarewolfAdmin /ADD
RUN NET localgroup "Warewolf Administrators" /ADD
RUN NET localgroup "Warewolf Administrators" WarewolfAdmin /ADD
EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]
RUN New-Item -Path Server -ItemType Directory
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"

RUN New-Item -Path "programdata\Warewolf" -ItemType Directory
RUN Copy-Item -Path \"Server\Resources - Release\*\" -Destination \"programdata\Warewolf\" -Recurse -Force
ADD NewAzureUser.bite \programdata\Warewolf\Resources

ENTRYPOINT Start-Process -FilePath "$env:SERVER_PATH" -ArgumentList "--interactive" -Verb RunAs -Wait

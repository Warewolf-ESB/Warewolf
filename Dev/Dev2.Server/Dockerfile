FROM mcr.microsoft.com/dotnet/framework/runtime:4.8

RUN NET localgroup "Warewolf Administrators" /ADD
EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]
RUN New-Item -Path Server -ItemType Directory
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"
ENV SERVER_USERNAME "WarewolfAdmin"
ENV SERVER_PASSWORD "W@rEw0lf@dm1n"

RUN New-Item -Path "programdata\Warewolf" -ItemType Directory
RUN Copy-Item -Path \"Server\Resources - Release\*\" -Destination \"programdata\Warewolf\" -Recurse -Force

HEALTHCHECK --interval=5s --timeout=3s CMD IF EXIST "Server\serverstarted" (EXIT 0) ELSE (EXIT 1)
ENTRYPOINT cmd /c NET user "$env:SERVER_USERNAME" "$env:SERVER_PASSWORD" /ADD /Y;cmd /c NET localgroup "Administrators" "$env:SERVER_USERNAME" /ADD;cmd /c NET localgroup "Warewolf Administrators" "$env:SERVER_USERNAME" /ADD;Start-Process -FilePath "$env:SERVER_PATH" -ArgumentList "--interactive" -Verb RunAs -Wait

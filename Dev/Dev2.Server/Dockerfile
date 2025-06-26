FROM mcr.microsoft.com/windows/servercore:ltsc2022

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
RUN New-Service -Name PerfMon \
                -BinaryPathName C:\Server\Warewolf.PerfMon.exe \
                -StartupType Automatic


CMD ["powershell", "-Command", ".\\Server\\StartAsService.ps1"]

FROM golang:1.12.6-windowsservercore-1809 as pauseloopbuilder
ADD pauseloop pauseloop
RUN cd pauseloop ; go mod init pauseloop ; go build -o pauseloop.exe .

FROM mcr.microsoft.com/dotnet/framework/runtime:4.8

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

COPY --from=pauseloopbuilder "c:\\gopath\\pauseloop\\pauseloop.exe" "c:\\windows\\system32\\pauseloop.exe"
CMD ["powershell", ".\\Server\\StartAsService.ps1 -NoExit"]

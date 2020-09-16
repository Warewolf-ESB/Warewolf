FROM golang:1.12.6-windowsservercore-1809 as pauseloopbuilder
ENV pausesrcdir="src\\github.com\\kubernetes-sigs\\windows-testing"
RUN mkdir $ENV:pausesrcdir ; cd $ENV:pausesrcdir ; git clone https://github.com/kubernetes-sigs/windows-testing.git . ; cd images\pause ; go build -o pauseloop.exe .

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

COPY --from=pauseloopbuilder "c:\\gopath\\src\\github.com\\kubernetes-sigs\\windows-testing\\images\\pause\\pauseloop.exe" "c:\\windows\\system32\\pauseloop.exe"
CMD .\Server\StartAs.ps1 -NoExit

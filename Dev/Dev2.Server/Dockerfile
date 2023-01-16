FROM golang:1.12.6-windowsservercore-1809 as pauseloopbuilder
ADD pauseloop pauseloop
RUN cd pauseloop ; go mod init pauseloop ; go build -o pauseloop.exe .

FROM mcr.microsoft.com/windows/nanoserver:ltsc22022

EXPOSE 3142
EXPOSE 3143

RUN mkdir Server
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"
ENV SERVER_USERNAME "WarewolfAdmin"
ENV SERVER_PASSWORD "W@rEw0lf@dm1n"

RUN mkdir "programdata\Warewolf"
RUN xcopy /s /y \"Server\Resources - Release\*\" \"programdata\Warewolf\"

COPY --from=pauseloopbuilder "c:\\gopath\\pauseloop\\pauseloop.exe" "c:\\windows\\system32\\pauseloop.exe"
CMD .\Server\AddAdminFromEnvironmentVariablesAndStart.bat

FROM golang:windowsservercore-ltsc2022 as pauseloopbuilder
ADD pauseloop pauseloop
RUN cd pauseloop ; go mod init pauseloop ; go build -o pauseloop.exe .

FROM mcr.microsoft.com/windows/servercore:ltsc2022
USER ContainerAdministrator

EXPOSE 3142
EXPOSE 3143

RUN mkdir Server
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_WORKINGDIR "C:\programdata\Warewolf"
ENV SERVER_LOG "%SERVER_WORKINGDIR%\Server Log\warewolf-server.log"
ENV SERVER_USERNAME "WarewolfAdmin"
ENV SERVER_PASSWORD "W@rEw0lf@dm1n"

RUN mkdir "programdata\Warewolf"
RUN xcopy /s /y "C:\Server\Resources - Release\" "%SERVER_WORKINGDIR%"

COPY --from=pauseloopbuilder "c:\\gopath\\pauseloop\\pauseloop.exe" "c:\\windows\\system32\\pauseloop.exe"
CMD .\Server\AddAdminFromEnvironmentVariablesAndStart.bat

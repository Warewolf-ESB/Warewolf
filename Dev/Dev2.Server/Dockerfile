FROM mcr.microsoft.com/windows/servercore:ltsc2019
USER ContainerAdministrator

EXPOSE 3142
EXPOSE 3143

RUN mkdir Server
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_WORKINGDIR "C:\programdata\Warewolf"
ENV SERVER_LOG "C:\programdata\Warewolf\Server Log\warewolf-server.log"
ENV SERVER_USERNAME "WarewolfAdmin"
ENV SERVER_PASSWORD "W@rEw0lf@dm1n"

RUN mkdir "programdata\Warewolf"
RUN xcopy /s /y "C:\Server\Resources - Release" "C:\programdata\Warewolf"

CMD .\Server\AddAdminFromEnvironmentVariablesAndStart.bat

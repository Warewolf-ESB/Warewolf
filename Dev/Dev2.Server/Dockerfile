FROM mcr.microsoft.com/windows/servercore:ltsc2019
USER ContainerAdministrator

RUN curl -o dotnet-sdk-installer.exe https://dotnetcli.azureedge.net/dotnet/Sdk/6.0.413/dotnet-sdk-6.0.413-win-x64.exe
RUN dotnet-sdk-installer.exe /quiet /install
RUN del dotnet-sdk-installer.exe

EXPOSE 3142
EXPOSE 3143

RUN mkdir Server
ADD . Server
ENV SERVER_PATH "Server\Warewolf Server.exe"
ENV SERVER_WORKINGDIR "C:\programdata\Warewolf"
ENV SERVER_LOG "C:\programdata\Warewolf\Server Log\warewolf-server.log"
ENV SERVER_USERNAME "WarewolfAdmin"
ENV SERVER_PASSWORD "W@rEw0lf@dm1n"

RUN mkdir "programdata\Warewolf\Resources"
RUN xcopy /s /y "C:\Server\Resources" "C:\programdata\Warewolf\Resources"
RUN mkdir "programdata\Warewolf\Tests"
RUN xcopy /s /y "C:\Server\Tests" "C:\programdata\Warewolf\Tests"

CMD .\Server\AddAdminFromEnvironmentVariablesAndStart.bat

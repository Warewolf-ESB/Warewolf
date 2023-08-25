FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
FROM mcr.microsoft.com/windows/servercore:ltsc2019
USER ContainerAdministrator

RUN powershell -Command Invoke-WebRequest -OutFile dotnet.zip https://dotnetcli.azureedge.net/dotnet/Runtime/6.0.20/dotnet-runtime-6.0.20-win-x64.zip
RUN powershell -Command Expand-Archive dotnet.zip -DestinationPath "C:\Program` Files\dotnet"
RUN powershell -Command Remove-Item -Force dotnet.zip
COPY --from=build ["C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App", "C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App/"]
COPY --from=build ["C:/Program Files/dotnet/shared/Microsoft.AspNetCore.App", "C:/Program Files/dotnet/shared/Microsoft.AspNetCore.App/"]

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

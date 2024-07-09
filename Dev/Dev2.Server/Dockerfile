FROM golang:1.12.6-windowsservercore-1809 as pauseloopbuilder
ADD pauseloop pauseloop
RUN cd pauseloop ; go mod init pauseloop ; go build -o pauseloop.exe .

FROM mcr.microsoft.com/dotnet/framework/runtime:4.8

EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]

###### DataDog Section ######
ARG TRACER_VERSION=2.53.2
ENV DD_TRACER_VERSION=$TRACER_VERSION
ENV ASPNETCORE_URLS=http://*.3142

ENV COR_ENABLE_PROFILING="1"
ENV COR_PROFILER="{846F5F1C-F9AE-4B07-969E-05C26BC060D8}"

ENV CORECLR_ENABLE_PROFILING="1"
ENV CORECLR_PROFILER=$COR_PROFILER

# We recommend always using the latest release and regularly updating: https://github.com/DataDog/dd-trace-dotnet/releases/latest
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Write-Host "Downloading Datadog .NET Tracer v$env:DD_TRACER_VERSION" ;\
    (New-Object System.Net.WebClient).DownloadFile('https://github.com/DataDog/dd-trace-dotnet/releases/download/v' + $env:DD_TRACER_VERSION + '/datadog-dotnet-apm-' + $env:DD_TRACER_VERSION + '-x64.msi', 'datadog-apm.msi') ;\
    Write-Host 'Installing Datadog .NET Tracer' ;\
    Start-Process -Wait msiexec -ArgumentList '/i datadog-apm.msi /quiet /qn /norestart /log datadog-apm-msi-installer.log' ; \
    Write-Host 'Datadog .NET Tracer installed, removing installer file' ; \
	Remove-Item 'datadog-apm.msi' ;
#####

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

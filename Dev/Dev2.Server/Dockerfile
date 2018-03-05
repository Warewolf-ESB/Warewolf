FROM microsoft/windowsservercore

RUN NET user WarewolfAdmin W@rEw0lf@dm1n /ADD
RUN NET localgroup "Administrators" WarewolfAdmin /ADD
RUN NET localgroup "Warewolf Administrators" /ADD
RUN NET localgroup "Warewolf Administrators" WarewolfAdmin /ADD
EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]
RUN New-Item -Path Server -ItemType Directory
ADD . Server
ENV SCRIPT_PATH "Server\Run Tests.ps1"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"

RUN New-Item -Path "programdata\Warewolf\Resources" -ItemType Directory
ADD NewAzureUser.bite programdata\Warewolf\Resources

ENTRYPOINT & $env:SCRIPT_PATH -StartServerAsConsole -ResourcesType Release

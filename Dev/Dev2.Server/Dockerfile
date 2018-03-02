FROM microsoft/windowsservercore

RUN NET user WarewolfAdmin W@rEw0lf@dm1n /ADD
RUN NET localgroup "Administrators" WarewolfAdmin /ADD
RUN NET localgroup "Warewolf Administrators" /ADD
RUN NET localgroup "Warewolf Administrators" WarewolfAdmin /ADD
EXPOSE 3142
EXPOSE 3143

SHELL ["powershell"]
RUN New-Item -Path Build -ItemType Directory
ADD . Build
ENV SCRIPT_PATH "Build\Run Tests.ps1"
ENV SERVER_LOG "programdata\Warewolf\Server Log\warewolf-server.log"

ENTRYPOINT & $env:SCRIPT_PATH -StartServerAsConsole -ResourcesType Release
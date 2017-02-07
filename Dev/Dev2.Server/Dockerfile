FROM microsoft/windowsservercore
RUN mkdir "C:\Program Files (x86)\Warewolf\Server"
COPY ["*", "C:\\Program Files (x86)\\Warewolf\\Server/"]
RUN sc create "Warewolf Server" binPath= "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe" start= auto
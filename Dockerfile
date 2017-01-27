FROM microsoft/windowsservercore
MAINTAINER ashley.lewis@dev2.co.za
SHELL ["powershell"]
RUN New-Item -ItemType Directory "C:\\build"
COPY ["Dev", "C:\\build"]

# Install MSBuild 14.0 and F# 4.0
RUN Invoke-WebRequest "https://download.microsoft.com/download/E/E/D/EEDF18A8-4AED-4CE0-BEBE-70A83094FC5A/BuildTools_Full.exe" -OutFile "$env:TEMP\BuildTools_Full.exe" -UseBasicParsing
RUN & "$env:TEMP\BuildTools_Full.exe" /Silent /Full | echo "Installing MSBuild."
RUN if (!(Test-Path \"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe\")) {Write-Host MSBuild did not install correctly!}
RUN Invoke-WebRequest "http://download.microsoft.com/download/9/1/2/9122D406-F1E3-4880-A66D-D6C65E8B1545/FSharp_Bundle.exe" -OutFile "$env:TEMP\FSharp_Bundle.exe" -UseBasicParsing
RUN & "$env:TEMP\FSharp_Bundle.exe" /install /quiet | echo "Installing F#."
RUN if (!(Test-Path \"C:\Program Files (x86)\Microsoft SDKs\F#\4.0\Framework\v4.0\fsc.exe\")) {Write-Host F# Compiler did not install correctly!}

# Using ADD instead of WebRequest
#ADD "https://download.microsoft.com/download/E/E/D/EEDF18A8-4AED-4CE0-BEBE-70A83094FC5A/BuildTools_Full.exe" "C:\\build\\BuildTools_Full.exe"
#RUN & "C:\build\BuildTools_Full.exe" /Silent /Full | echo "Installing MSBuild."
#ADD "https://download.microsoft.com/download/E/A/3/EA38D9B8-E00F-433F-AAB5-9CDA28BA5E7D/FSharp_Bundle.exe" "C:\\build\\FSharp_Bundle.exe"
#RUN & "C:\build\FSharp_Bundle.exe" /install /quiet | echo "Installing F#."

# Install NuGet
RUN Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "C:\windows\nuget.exe" -UseBasicParsing

# Compile Warewolf Server
RUN ["C:\\windows\\nuget.exe", "restore", "C:\\build\\Server.sln", "-MsbuildVersion", "14"]
RUN ["C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe", "C:\\build\\Server.sln",  "/p:Platform=\"Any CPU\";Configuration=\"Release\"", "/maxcpucount"]

# Setup Server Service
RUN cmd /c sc create \"Warewolf Server\" binPath= \"C:\build\Dev2.Server\bin\Release\Warewolf Server.exe\" start= auto
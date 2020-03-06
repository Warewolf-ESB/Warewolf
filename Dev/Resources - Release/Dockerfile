FROM warewolfserver/warewolfserver
SHELL ["powershell"]
RUN Remove-Item -r -Force \"Server\Resources\*\"
RUN Remove-Item -r -Force \"Server\Resources - Release\"
RUN Remove-Item -r -Force \"programdata\Warewolf\Resources\"
RUN Remove-Item -r -Force \"programdata\Warewolf\Tests\"
ADD Resources programdata\Warewolf
ADD Tests programdata\Warewolf
CMD .\Server\StartAs.ps1 -NoExit
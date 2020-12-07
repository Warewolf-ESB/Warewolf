FROM warewolfserver/msbuild
COPY .\ /Dev
RUN "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" /Dev/AcceptanceTesting.sln
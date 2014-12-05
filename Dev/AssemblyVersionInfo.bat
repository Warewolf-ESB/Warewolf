SET MajorMinorVersion=0.4

CD %~dp0
GIT update-index --assume-unchanged AssemblyCommonInfo.cs
echo using System.Reflection;>AssemblyCommonInfo.cs
echo [assembly: AssemblyCompany("Warewolf")]>>AssemblyCommonInfo.cs
echo [assembly: AssemblyProduct("Warewolf ESB")]>>AssemblyCommonInfo.cs
echo [assembly: AssemblyCopyright("Copyright Warewolf 2013")]>>AssemblyCommonInfo.cs
echo [assembly: AssemblyVersion("%MajorMinorVersion%.*")]>>AssemblyCommonInfo.cs
for /f "delims=" %%a in ('git rev-parse head') do @echo [assembly: AssemblyInformationalVersion("%%a")]>>AssemblyCommonInfo.cs
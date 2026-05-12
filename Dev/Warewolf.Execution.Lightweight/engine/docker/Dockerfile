# escape=`
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-nanoserver-ltsc2022 AS final

WORKDIR C:\home\site\wwwroot
COPY ./publish .



ENV AzureWebJobsScriptRoot=C:\home\site\wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
ENV ASPNETCORE_ENVIRONMENT=Development
ENV AZURE_TENANT_ID=
ENV DEBUG_AZURE_KEYVAULT_SECRET=
ENV ENABLEELASTICSEARCHLOGGING=true
ENV ENABLECONSOLELOGGING=true
ENV EXECUTIONLOGLEVEL=4
EXPOSE 80
EXPOSE 7071
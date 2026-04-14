FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice AS final
WORKDIR /home/site/wwwroot

COPY ./publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Development \
    ENABLECONSOLELOGGING=true \
    ENABLEELASTICSEARCHLOGGING=true

EXPOSE 80
EXPOSE 7071


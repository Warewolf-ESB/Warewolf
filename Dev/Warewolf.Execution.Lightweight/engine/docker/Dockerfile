FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice AS final

WORKDIR /home/site/wwwroot
COPY ./publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Development \
    AZURE_TENANT_ID= \
    DEBUG_AZURE_KEYVAULT_SECRET="" \
    ENABLEELASTICSEARCHLOGGING=true \
    ENABLECONSOLELOGGING=true \
    EXECUTIONLOGLEVEL=4
EXPOSE 80
EXPOSE 7071
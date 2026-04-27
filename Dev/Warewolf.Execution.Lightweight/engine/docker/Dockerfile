FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice AS final

WORKDIR /home/site/wwwroot
COPY ./publish .



ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Development \
    AZURE_TENANT_ID=ca0cc53b-9af4-4067-bcdf-be9c648450d1 \
    DEBUG_AZURE_KEYVAULT_SECRET="{version:1,keyId:79fc2c4d-cb0a-4d15-985b-5be60cc5d235,key:QLdcnqh/MPtmbxYbuOMUSJ/U3HEFdDdl+gX/1ghjCIQ=,created:2026-04-08T23:36:56.6266156+05:30}" \
    ENABLEELASTICSEARCHLOGGING=true \
    ENABLECONSOLELOGGING=true
EXPOSE 80
EXPOSE 7071
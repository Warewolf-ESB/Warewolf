##########################################################
# Warewolf Execution Engine - Azure Auth Provisioning
#
# Provisions:
#   * Resource group + storage account
#   * Azure Functions app (Consumption, .NET 8 isolated)
#   * Entra ID app registration with app roles matching
#     the Warewolf secure.config WindowsGroup role values
#   * Easy Auth (authsettingsV2) in AllowAnonymous global mode:
#     /public/* open to all; /secure/* enforced by C# middleware
#
# Prerequisites:
#   az cli 2.40+, PowerShell 7+
#   az login && az account set --subscription <id>
##########################################################

# ── Configuration ──────────────────────────────────────
$RESOURCE_GROUP  = "rg-warewolf-engine"
$LOCATION        = "australiaeast"
$STORAGE_ACCOUNT = "stwwengine"          # 3-24 lowercase chars, globally unique
$FUNCTION_APP    = "wwengine"
$APP_REG_NAME    = "wwengine-auth"
$SUBSCRIPTION_ID = az account show --query "id" -o tsv

Write-Host "`n[INFO] Starting Warewolf Engine provisioning..." -ForegroundColor Cyan

# ── 1. Resource Group ───────────────────────────────────
Write-Host "`n[1/8] Creating resource group '$RESOURCE_GROUP'..."
az group create `
    --name     $RESOURCE_GROUP `
    --location $LOCATION

# ── 2. Storage Account ─────────────────────────────────
Write-Host "[2/8] Creating storage account '$STORAGE_ACCOUNT'..."
az storage account create `
    --name                     $STORAGE_ACCOUNT `
    --resource-group           $RESOURCE_GROUP `
    --location                 $LOCATION `
    --sku                      Standard_LRS `
    --allow-blob-public-access false

# ── 3. Function App (Consumption / free tier) ──────────
Write-Host "[3/8] Creating function app '$FUNCTION_APP' (.NET 8 isolated, consumption)..."
az functionapp create `
    --resource-group            $RESOURCE_GROUP `
    --consumption-plan-location $LOCATION `
    --runtime                   dotnet-isolated `
    --runtime-version           8 `
    --functions-version         4 `
    --name                      $FUNCTION_APP `
    --storage-account           $STORAGE_ACCOUNT `
    --os-type                   Windows

# ── 4. Entra ID App Registration ───────────────────────
Write-Host "[4/8] Registering Entra ID application '$APP_REG_NAME'..."
$redirectUri = "https://$FUNCTION_APP.azurewebsites.net/.auth/login/aad/callback"

$CLIENT_ID = az ad app create `
    --display-name      $APP_REG_NAME `
    --sign-in-audience  AzureADMyOrg `
    --web-redirect-uris $redirectUri `
    --query "appId" -o tsv

Write-Host "   App (client) ID : $CLIENT_ID" -ForegroundColor Green

$TENANT_ID = az account show --query "tenantId" -o tsv
Write-Host "   Tenant ID       : $TENANT_ID" -ForegroundColor Green

# ── 5. Client Secret ────────────────────────────────────
Write-Host "[5/8] Creating client secret (2-year expiry)..."
$CLIENT_SECRET = az ad app credential reset `
    --id           $CLIENT_ID `
    --display-name "wwengine-secret" `
    --years        2 `
    --query        "password" -o tsv

# ── 6. App Roles ────────────────────────────────────────
# The "value" field of each role MUST match the WindowsGroup string you enter in
# Warewolf Server's secure.config for each permission entry.
Write-Host "[6/8] Adding Warewolf app roles to Entra manifest..."

$appRoles = @"
[
  {
    "allowedMemberTypes": ["User","Application"],
    "description": "Full administrator access to all Warewolf workflows",
    "displayName": "WarewolfAdministrator",
    "id": "$([System.Guid]::NewGuid())",
    "isEnabled": true,
    "value": "WarewolfAdministrator"
  },
  {
    "allowedMemberTypes": ["User"],
    "description": "Can execute and contribute to Warewolf workflows",
    "displayName": "WorkflowContributor",
    "id": "$([System.Guid]::NewGuid())",
    "isEnabled": true,
    "value": "WorkflowContributor"
  },
  {
    "allowedMemberTypes": ["User"],
    "description": "Can execute Warewolf workflows via /secure/* endpoints",
    "displayName": "WorkflowExecutor",
    "id": "$([System.Guid]::NewGuid())",
    "isEnabled": true,
    "value": "WorkflowExecutor"
  },
  {
    "allowedMemberTypes": ["User"],
    "description": "Read-only access to public Warewolf endpoints",
    "displayName": "WorkflowViewer",
    "id": "$([System.Guid]::NewGuid())",
    "isEnabled": true,
    "value": "WorkflowViewer"
  }
]
"@

$appRoles | Out-File -FilePath "./approles.json" -Encoding utf8
az ad app update --id $CLIENT_ID --app-roles "@approles.json"
Remove-Item "./approles.json"

# ── 7. API Identifier URI ───────────────────────────────
Write-Host "[7/8] Setting API identifier URI 'api://$CLIENT_ID'..."
az ad app update `
    --id              $CLIENT_ID `
    --identifier-uris "api://$CLIENT_ID"

# ── 8. Easy Auth (authsettingsV2) ───────────────────────
Write-Host "[8/8] Configuring Easy Auth on Function App..."

# MICROSOFT_PROVIDER_AUTHENTICATION_SECRET is the App Setting name Easy Auth
# looks for automatically — do not rename it.
az functionapp config appsettings set `
    --name           $FUNCTION_APP `
    --resource-group $RESOURCE_GROUP `
    --settings       "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET=$CLIENT_SECRET"

# These settings feed EntraTokenValidator / SecureConfigLoader in the C# middleware.
az functionapp config appsettings set `
    --name           $FUNCTION_APP `
    --resource-group $RESOURCE_GROUP `
    --settings `
        "WAREWOLF_ENTRA_TENANT_ID=$TENANT_ID" `
        "WAREWOLF_ENTRA_AUDIENCE=api://$CLIENT_ID" `
        "AZURE_TENANT_ID=$TENANT_ID"

# Apply authsettingsV2:
#   globalValidation.requireAuthentication = false + AllowAnonymous
#   => Easy Auth validates tokens when present but does NOT block anonymous requests.
#   => The C# middleware pipeline (EasyAuthRedirectMiddleware) enforces auth on /secure/*.
$authConfig = @"
{
  "platform": { "enabled": true },
  "globalValidation": {
    "requireAuthentication": false,
    "unauthenticatedClientAction": "AllowAnonymous"
  },
  "identityProviders": {
    "azureActiveDirectory": {
      "enabled": true,
      "registration": {
        "openIdIssuer": "https://login.microsoftonline.com/$TENANT_ID/v2.0",
        "clientId": "$CLIENT_ID",
        "clientSecretSettingName": "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      },
      "validation": {
        "allowedAudiences": ["api://$CLIENT_ID"]
      }
    }
  },
  "login": {
    "routes": { "logoutEndpoint": "/.auth/logout" },
    "tokenStore": { "enabled": true }
  }
}
"@

$authConfig | Out-File -FilePath "./authconfig.json" -Encoding utf8
az rest `
    --method PUT `
    --url    "https://management.azure.com/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Web/sites/$FUNCTION_APP/config/authsettingsV2?api-version=2022-03-01" `
    --body   "@authconfig.json"
Remove-Item "./authconfig.json"

# ── Summary ─────────────────────────────────────────────
Write-Host "`n[OK] Provisioning complete!" -ForegroundColor Green
Write-Host @"
=====================================================
  Function App URL : https://$FUNCTION_APP.azurewebsites.net
  Tenant ID        : $TENANT_ID
  Client (App) ID  : $CLIENT_ID
  Client Secret    : [stored in App Settings]
  Login URL        : https://$FUNCTION_APP.azurewebsites.net/.auth/login/aad
  Claims check     : https://$FUNCTION_APP.azurewebsites.net/.auth/me
=====================================================

Next steps
----------
1. Assign Entra users / groups to app roles:
     Entra ID -> Enterprise Applications -> $APP_REG_NAME -> Users and groups

2. Set 'Assignment required = Yes' (recommended):
     Enterprise Applications -> $APP_REG_NAME -> Properties

3. Configure secure.config in Warewolf Server with matching WindowsGroup values:
     WarewolfAdministrator | WorkflowContributor | WorkflowExecutor | WorkflowViewer

4. Update local.settings.json for local development:
     AZURE_TENANT_ID               = $TENANT_ID
     WAREWOLF_ENTRA_TENANT_ID      = $TENANT_ID
     WAREWOLF_ENTRA_AUDIENCE       = api://$CLIENT_ID
     MICROSOFT_PROVIDER_AUTHENTICATION_SECRET = <value from step 5>

5. Deploy the function app:
     dotnet publish -c Release -o ./publish
     Compress-Archive -Path ./publish/* -DestinationPath ./wwengine.zip -Force
     az functionapp deployment source config-zip ``
         --resource-group $RESOURCE_GROUP --name $FUNCTION_APP --src ./wwengine.zip
"@

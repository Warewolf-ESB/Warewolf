# Do not save this session's commands (or anything pasted) to Cloud Shell history
Set-PSReadLineOption -HistorySaveStyle SaveNothing

$now   = (Get-Date).ToUniversalTime().ToString('o')
$bytes = [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
$ring  = [ordered]@{
    version      = 1
    keyId        = [guid]::NewGuid().Guid
    key          = [Convert]::ToBase64String($bytes)
    created      = $now
    previousKeys = @()
}
[Array]::Clear($bytes)
$json = $ring | ConvertTo-Json -Compress -Depth 5

# Validate exactly as the engine does: valid JSON, key = Base64 of 32 bytes
$c = $json | ConvertFrom-Json
if ([Convert]::FromBase64String($c.key).Length -ne 32) { throw 'Primary key is not 32 bytes' }
"Primary: $($c.keyId)   previousKeys: $(@($c.previousKeys).Count)"
$json

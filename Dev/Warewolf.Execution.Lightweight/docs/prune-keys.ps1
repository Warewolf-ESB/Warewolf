# Do not save this session's commands (or anything pasted) to Cloud Shell history
Set-PSReadLineOption -HistorySaveStyle SaveNothing

# Paste the CURRENT secret value between the quotes (Portal > Show Secret Value > copy)
$current = '<PASTE-CURRENT-SECRET-VALUE>'

# Keep the primary key exactly as it is; drop every retired key
$c       = $current | ConvertFrom-Json
$created = $c.created
if ($created -is [datetime]) { $created = $created.ToUniversalTime().ToString('o') }
$ring = [ordered]@{
    version      = $c.version
    keyId        = $c.keyId
    key          = $c.key
    created      = "$created"
    previousKeys = @()
}
$json = $ring | ConvertTo-Json -Compress -Depth 5

$check = $json | ConvertFrom-Json
if ([Convert]::FromBase64String($check.key).Length -ne 32) { throw 'Primary key is not 32 bytes' }
"Primary kept: $($c.keyId)   removed: $(@($c.previousKeys.keyId) -join ', ')"
$json

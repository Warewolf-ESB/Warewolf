# Do not save this session's commands (or anything pasted) to Cloud Shell history
Set-PSReadLineOption -HistorySaveStyle SaveNothing

# Paste the CURRENT secret value between the quotes (Portal > Show Secret Value > copy)
$current = '<PASTE-CURRENT-SECRET-VALUE>'

$old = $current | ConvertFrom-Json          # stops here if the value is not valid JSON
$now = (Get-Date).ToUniversalTime().ToString('o')

# The current primary becomes the newest previous key; existing previous keys are kept
$prev = @([ordered]@{ keyId = $old.keyId; key = $old.key; retired = $now })
foreach ($p in @($old.previousKeys)) {
    if (-not $p) { continue }
    $retired = $p.retired
    if ($retired -is [datetime]) { $retired = $retired.ToUniversalTime().ToString('o') }
    $prev += [ordered]@{ keyId = $p.keyId; key = $p.key; retired = "$retired" }
}

$bytes = [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
$ring  = [ordered]@{
    version      = 2
    keyId        = [guid]::NewGuid().Guid
    key          = [Convert]::ToBase64String($bytes)
    created      = $now
    previousKeys = $prev
}
[Array]::Clear($bytes)
$json = $ring | ConvertTo-Json -Compress -Depth 5

# Validate: every key = Base64 of 32 bytes, no duplicate keyIds
$c   = $json | ConvertFrom-Json
$all = @($c) + @($c.previousKeys)
foreach ($k in $all) {
    if ([Convert]::FromBase64String($k.key).Length -ne 32) { throw "Key $($k.keyId) is not 32 bytes" }
}
if (@($all.keyId | Sort-Object -Unique).Count -ne $all.Count) { throw 'Duplicate keyId in ring' }
"Primary: $($c.keyId)   previousKeys: $(@($c.previousKeys.keyId) -join ', ')"
$json

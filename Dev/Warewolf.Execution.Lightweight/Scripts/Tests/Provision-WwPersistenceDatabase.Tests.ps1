#Requires -Version 7.0
<#
    Pester 5 suite for Provision-WwPersistenceDatabase.ps1.

    Everything here runs offline: -LoadFunctionsOnly dot-sources the pure helpers and returns before
    the script reaches its parameter guards, its az calls or any file write. No Azure, no SQL.

    The behaviours worth pinning are the DECISIONS, not the az plumbing: which FQDN maps to which
    server resource, what a login mismatch means, which failures are allowed to fail a build, and
    whether a password can leak into a summary.
#>

BeforeAll {
    $script:ScriptPath = Join-Path (Split-Path -Parent $PSScriptRoot) 'Provision-WwPersistenceDatabase.ps1'
    . $script:ScriptPath -LoadFunctionsOnly
}

Describe 'Get-WwSqlServerFromFqdn' {

    BeforeAll {
        $script:Servers = @(
            [pscustomobject]@{ name = 'other';  resourceGroup = 'rg-a'; location = 'southafricanorth'
                               fullyQualifiedDomainName = 'other.database.windows.net' }
            [pscustomobject]@{ name = 'uatsql'; resourceGroup = 'rg-uat'; location = 'southafricanorth'
                               fullyQualifiedDomainName = 'uatsql.database.windows.net'
                               administratorLogin = 'warewolfadmin' }
        )
    }

    It 'resolves the server whose FQDN matches the connection string' {
        # The resource group is the point: it is NOT derivable from the hostname, and every later
        # az call needs it.
        $s = Get-WwSqlServerFromFqdn -Servers $script:Servers -Fqdn 'uatsql.database.windows.net'
        $s.name          | Should -Be 'uatsql'
        $s.resourceGroup | Should -Be 'rg-uat'
    }

    It 'matches case-insensitively and tolerates a trailing dot' {
        # A fully-qualified DNS name may legitimately carry the root dot.
        (Get-WwSqlServerFromFqdn -Servers $script:Servers -Fqdn 'UATSQL.Database.Windows.Net.').name |
            Should -Be 'uatsql'
    }

    It 'returns $null when nothing matches, rather than guessing at the first label' {
        Get-WwSqlServerFromFqdn -Servers $script:Servers -Fqdn 'nosuch.database.windows.net' |
            Should -BeNullOrEmpty
    }

    It 'returns $null for an empty server list' {
        Get-WwSqlServerFromFqdn -Servers @() -Fqdn 'uatsql.database.windows.net' | Should -BeNullOrEmpty
        Get-WwSqlServerFromFqdn -Servers $null -Fqdn 'uatsql.database.windows.net' | Should -BeNullOrEmpty
    }

    It 'THROWS on ambiguity instead of picking one' {
        # Two servers answering to one FQDN means the caller is not describing what it thinks it is;
        # silently taking the first would provision into the wrong resource group.
        $dupes = @(
            [pscustomobject]@{ name = 'a'; resourceGroup = 'rg-a'; fullyQualifiedDomainName = 'dup.database.windows.net' }
            [pscustomobject]@{ name = 'b'; resourceGroup = 'rg-b'; fullyQualifiedDomainName = 'dup.database.windows.net' }
        )
        { Get-WwSqlServerFromFqdn -Servers $dupes -Fqdn 'dup.database.windows.net' } |
            Should -Throw -ExpectedMessage '*Ambiguous*'
    }
}

Describe 'Get-WwAdminLoginCheck' {

    It 'PASSES when the engine login IS the server administrator' {
        # Only then is the login dbo in a database created through the control plane.
        $c = Get-WwAdminLoginCheck -ActualAdminLogin 'warewolfadmin' -ExpectedLogin 'warewolfadmin' -DatabaseName 'hangfire'
        $c.Class  | Should -Be 'BLOCKER'
        $c.Status | Should -Be 'PASS'
    }

    It 'FAILS as a BLOCKER when the logins differ' {
        $c = Get-WwAdminLoginCheck -ActualAdminLogin 'sqladmin' -ExpectedLogin 'profileruser' -DatabaseName 'hangfire'
        $c.Class  | Should -Be 'BLOCKER'
        $c.Status | Should -Be 'FAIL'
    }

    It 'carries the exact CREATE USER statement an operator must run once' {
        # This script has no SQL client, so the remediation has to be actionable by hand.
        # -Match, not -BeLike: the statement is full of square brackets, which -BeLike reads as
        # character classes and would quietly match almost anything.
        $c = Get-WwAdminLoginCheck -ActualAdminLogin 'sqladmin' -ExpectedLogin 'profileruser' -DatabaseName 'uat-hangfire'
        $c.Remediation | Should -Match ([regex]::Escape('CREATE USER [profileruser] FOR LOGIN [profileruser];'))
        $c.Remediation | Should -Match ([regex]::Escape('ALTER ROLE db_owner ADD MEMBER [profileruser];'))
        $c.Remediation | Should -Match ([regex]::Escape('[uat-hangfire]'))
    }

    It 'explains WHY db_owner is needed, not merely that it is' {
        # PrepareSchemaIfNecessary=true means the engine creates the Hangfire schema itself.
        (Get-WwAdminLoginCheck -ActualAdminLogin 'a' -ExpectedLogin 'b' -DatabaseName 'd').Remediation |
            Should -BeLike '*PrepareSchemaIfNecessary*'
    }

    It 'treats a server with NO SQL administrator as a BLOCKER (Entra-only server)' {
        $c = Get-WwAdminLoginCheck -ActualAdminLogin '' -ExpectedLogin 'warewolfadmin' -DatabaseName 'hangfire'
        $c.Status      | Should -Be 'FAIL'
        $c.Remediation | Should -BeLike '*Entra-only*'
    }
}

Describe 'Get-WwProvisionVerdict' {

    It 'passes when every check passed' {
        $checks = @(
            New-WwProvisionCheck -Class 'BLOCKER' -Name 'a' -Status 'PASS'
            New-WwProvisionCheck -Class 'REPORT'  -Name 'b' -Status 'INFO'
        )
        (Get-WwProvisionVerdict -Checks $checks).Passed | Should -BeTrue
    }

    It 'does NOT fail on an EXPECTED-LATER absence' {
        # The whole point of the class: a preflight run against a not-yet-provisioned environment
        # must not read as a broken pipeline.
        $checks = @(
            New-WwProvisionCheck -Class 'BLOCKER'        -Name 'admin login' -Status 'PASS'
            New-WwProvisionCheck -Class 'EXPECTED-LATER' -Name 'database exists' -Status 'FAIL'
            New-WwProvisionCheck -Class 'EXPECTED-LATER' -Name 'firewall rule'  -Status 'FAIL'
        )
        $v = Get-WwProvisionVerdict -Checks $checks
        $v.Passed              | Should -BeTrue
        $v.ExpectedLaterAbsent | Should -Be 2
    }

    It 'fails on a BLOCKER, and names the failing check' {
        $checks = @(
            New-WwProvisionCheck -Class 'BLOCKER'        -Name 'SQL admin login matches the engine login' -Status 'FAIL'
            New-WwProvisionCheck -Class 'EXPECTED-LATER' -Name 'database exists' -Status 'FAIL'
        )
        $v = Get-WwProvisionVerdict -Checks $checks
        $v.Passed  | Should -BeFalse
        $v.Message | Should -BeLike '*SQL admin login matches the engine login*'
    }

    It 'never fails on a REPORT row, whatever its status' {
        $checks = @(New-WwProvisionCheck -Class 'REPORT' -Name 'roles' -Status 'INFO')
        (Get-WwProvisionVerdict -Checks $checks).Passed | Should -BeTrue
    }
}

Describe 'Get-WwDatabaseSkuName' {

    It 'derives the SERVERLESS SKU, where the _S_ is the whole difference' {
        # GP_S_Gen5_1 auto-pauses to zero cost; GP_Gen5_1 bills continuously. Derived, never typed.
        Get-WwDatabaseSkuName -Edition 'GeneralPurpose' -ComputeModel 'Serverless' -Family 'Gen5' -Capacity 1 |
            Should -Be 'GP_S_Gen5_1'
    }

    It 'derives the provisioned SKU when the compute model is not serverless' {
        Get-WwDatabaseSkuName -Edition 'GeneralPurpose' -ComputeModel 'Provisioned' -Family 'Gen5' -Capacity 4 |
            Should -Be 'GP_Gen5_4'
    }

    It 'supports the other editions' {
        Get-WwDatabaseSkuName -Edition 'BusinessCritical' -ComputeModel 'Provisioned' -Family 'Gen5' -Capacity 2 |
            Should -Be 'BC_Gen5_2'
    }

    It 'throws on an edition it cannot spell, rather than inventing a SKU' {
        { Get-WwDatabaseSkuName -Edition 'Nonsense' -ComputeModel 'Serverless' -Family 'Gen5' -Capacity 1 } |
            Should -Throw -ExpectedMessage '*Unsupported -Edition*'
    }
}

Describe 'New-WwSqlDatabaseWhatIfTemplate' {

    It 'describes EXACTLY the database the real run would create' {
        # what-if only answers for what it is given: probing a different name or SKU would prove the
        # pipeline may create something, not that it may create THIS.
        $json = New-WwSqlDatabaseWhatIfTemplate -ServerName 'uatsql' -DatabaseName 'uat-hangfire' `
            -Location 'southafricanorth' -SkuName 'GP_S_Gen5_1' -Edition 'GeneralPurpose' `
            -Family 'Gen5' -Capacity 1
        $t = $json | ConvertFrom-Json
        $t.resources[0].type     | Should -Be 'Microsoft.Sql/servers/databases'
        $t.resources[0].name     | Should -Be 'uatsql/uat-hangfire'
        $t.resources[0].sku.name | Should -Be 'GP_S_Gen5_1'
        $t.resources[0].location | Should -Be 'southafricanorth'
    }

    It 'carries the serverless properties so the probe matches the real shape' {
        $t = (New-WwSqlDatabaseWhatIfTemplate -ServerName 's' -DatabaseName 'd' -Location 'l' `
                -SkuName 'GP_S_Gen5_1' -Edition 'GeneralPurpose' -Family 'Gen5' -Capacity 1 `
                -MinCapacity 0.5 -AutoPauseDelayMinutes 60) | ConvertFrom-Json
        $t.resources[0].properties.autoPauseDelay | Should -Be 60
        $t.resources[0].properties.minCapacity    | Should -Be 0.5
    }

    It 'emits a valid ARM template envelope' {
        $t = (New-WwSqlDatabaseWhatIfTemplate -ServerName 's' -DatabaseName 'd' -Location 'l' `
                -SkuName 'GP_S_Gen5_1' -Edition 'GeneralPurpose' -Family 'Gen5' -Capacity 1) | ConvertFrom-Json
        $t.contentVersion | Should -Be '1.0.0.0'
        $t.'$schema'      | Should -BeLike '*deploymentTemplate.json#'
    }
}

Describe 'Test-WwAzAuthorizationFailure' {

    It 'recognises the shapes Azure uses to say "not allowed"' {
        # Only these justify reporting a PERMISSION problem; anything else is not evidence.
        foreach ($msg in @(
            "ERROR: (AuthorizationFailed) The client 'x' with object id 'y' does not have authorization to perform action",
            'Forbidden',
            'RequestDisallowedByPolicy',
            'InsufficientPrivileges'
        )) {
            Test-WwAzAuthorizationFailure -Output $msg | Should -BeTrue -Because "'$msg' is an authorization failure"
        }
    }

    It 'does NOT read an unrelated failure as a permission problem' {
        # A transient or malformed-input failure says nothing about RBAC, and reporting it as a
        # BLOCKER would send someone to fix permissions that were never wrong.
        foreach ($msg in @(
            'ERROR: The resource group could not be found.',
            'Connection timed out',
            'InvalidTemplateDeployment: the template is malformed'
        )) {
            Test-WwAzAuthorizationFailure -Output $msg | Should -BeFalse -Because "'$msg' is not about authorization"
        }
    }

    It 'treats empty output as no evidence' {
        Test-WwAzAuthorizationFailure -Output ''    | Should -BeFalse
        Test-WwAzAuthorizationFailure -Output $null | Should -BeFalse
    }
}

Describe 'Connection string assembly and masking' {

    It 'builds the shape the DbSource .bite carries' {
        $cs = ConvertTo-WwPersistenceConnectionString -ServerFqdn 'uatsql.database.windows.net' `
                -DatabaseName 'uat-hangfire' -UserId 'warewolfadmin' -Password 'p@ss;word'
        $cs | Should -BeLike 'Data Source=uatsql.database.windows.net,1433;*'
        $cs | Should -BeLike '*Initial Catalog=uat-hangfire;*'
        $cs | Should -BeLike '*User ID=warewolfadmin;*'
        $cs | Should -BeLike '*Encrypt=True*'
    }

    It 'masks ONLY the password, keeping what a reader needs to verify the target' {
        $masked = Hide-WwConnectionStringPassword -ConnectionString (
            ConvertTo-WwPersistenceConnectionString -ServerFqdn 'uatsql.database.windows.net' `
                -DatabaseName 'uat-hangfire' -UserId 'warewolfadmin' -Password 'sup3rs3cret')
        $masked | Should -Not -BeLike '*sup3rs3cret*'
        $masked | Should -BeLike '*Password=***'
        $masked | Should -BeLike '*Initial Catalog=uat-hangfire*'
        $masked | Should -BeLike '*User ID=warewolfadmin*'
    }

    It 'masks case-insensitively and stops at the delimiter' {
        $masked = Hide-WwConnectionStringPassword -ConnectionString 'Server=s;PASSWORD=abc;Encrypt=True'
        $masked | Should -Be 'Server=s;PASSWORD=***;Encrypt=True'
    }
}

Describe 'New-WwProvisionCheck' {

    It 'rejects a class outside the preflight vocabulary' {
        # The pipeline's preflight report is read by class; an invented class would be silently
        # ignored by the verdict.
        { New-WwProvisionCheck -Class 'WARNING' -Name 'n' -Status 'PASS' } | Should -Throw
    }

    It 'rejects a status outside PASS/FAIL/INFO' {
        { New-WwProvisionCheck -Class 'BLOCKER' -Name 'n' -Status 'MAYBE' } | Should -Throw
    }
}

Describe 'Provision-WwPersistenceDatabase.ps1 parameter guards' {

    It 'does not prompt when dot-sourced for helpers only' {
        { . $script:ScriptPath -LoadFunctionsOnly } | Should -Not -Throw
    }

    It 'requires -SqlServerFqdn, -DatabaseName and -ExpectedAdminLogin' {
        { & $script:ScriptPath -DatabaseName 'd' -ExpectedAdminLogin 'a' } |
            Should -Throw -ExpectedMessage '*-SqlServerFqdn is required*'
        { & $script:ScriptPath -SqlServerFqdn 'f' -ExpectedAdminLogin 'a' } |
            Should -Throw -ExpectedMessage '*-DatabaseName is required*'
        { & $script:ScriptPath -SqlServerFqdn 'f' -DatabaseName 'd' } |
            Should -Throw -ExpectedMessage '*-ExpectedAdminLogin is required*'
    }
}

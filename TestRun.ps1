param(
  [Parameter(Mandatory=$true)]
  [String[]] $Projects, 
  [String] $Category,
  [String[]] $Categories,
  [String[]] $ExcludeProjects = @(), 
  [String[]] $ExcludeCategories,
  [String] $TestsToRun="${bamboo.TestsToRun}",
  [String] $VSTestPath="${bamboo.capability.system.builder.devenv.Visual Studio 2019}",
  [String] $NuGet="${bamboo.capability.system.builder.command.NuGet}",
  [String] $MSBuildPath="${bamboo.capability.system.builder.msbuild.MSBuild v16.0}",
  [int] $RetryCount=${bamboo.RetryCount},
  [String] $PreTestRunScript,
  [String] $PostTestRunScript,
  [switch] $InContainer,
  [String] $InContainerVersion="latest",
  [String] $InContainerCommitID="latest",
  [switch] $Coverage,
  [switch] $RetryRebuild,
  [String] $UNCPassword,
  [switch] $StartFTPServer,
  [switch] $StartFTPSServer,
  [switch] $CreateUNCPath,
  [switch] $UseRegionalSettings,
  [switch] $CreateLocalSchedulerAdmin,
  [switch] $STA
)
function Start-FTPServer {
	if (!(Test-Path "C:\ftp_home\dev2\FORUNZIPTESTING")) {
		mkdir "C:\ftp_home\dev2\FORUNZIPTESTING"
	}
	pip install pyftpdlib
	if (!(Test-Path "C:\ftp_entrypoint.py")) {
@"
import os, random, string

from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import FTPHandler
from pyftpdlib.servers import FTPServer

PASSIVE_PORTS = '17000-17007'

def main():
	authorizer = DummyAuthorizer()
	user_dir = "C:/ftp_home/dev2"
	if not os.path.isdir(user_dir):
		os.mkdir(user_dir)
	authorizer.add_user("dev2", "Q/ulw&]", user_dir, perm="elradfmw")

	handler = FTPHandler
	handler.authorizer = authorizer
	handler.permit_foreign_addresses = True

	passive_ports = list(map(int, PASSIVE_PORTS.split('-')))
	handler.passive_ports = range(passive_ports[0], passive_ports[1])

	server = FTPServer(('0.0.0.0', 21), handler)
	server.serve_forever()
	
if __name__ == '__main__':
	main()
"@ | Out-File -LiteralPath "C:\ftp_entrypoint.py" -Encoding utf8 -Force
	}
	pythonw -u "C:\ftp_entrypoint.py"
}
function Start-FTPSServer {
	if (!(Test-Path "C:\ftps_home\dev2\FORFILERENAMETESTING")) {
		mkdir "C:\ftps_home\dev2\FORFILERENAMETESTING"
	}
	if (!(Test-Path "C:\ftps_home\dev2\FORUNZIPTESTING")) {
		mkdir "C:\ftps_home\dev2\FORUNZIPTESTING"
	}
	if (!(Test-Path "C:\cert.crt")) {
@"
-----BEGIN CERTIFICATE-----
MIID+TCCAuGgAwIBAgIUMjnF+Uh4NhKoRO425/Sgjbs7xs0wDQYJKoZIhvcNAQEL
BQAwgYsxCzAJBgNVBAYTAlpBMQwwCgYDVQQIDANLWk4xEjAQBgNVBAcMCUhpbGxj
cmVzdDERMA8GA1UECgwIV2FyZXdvbGYxDzANBgNVBAsMBkRldk9wczEUMBIGA1UE
AwwLb3Bzd29sZi5jb20xIDAeBgkqhkiG9w0BCQEWEWFkbWluQG9wc3dvbGYuY29t
MB4XDTIxMDQxODA2MzYzMVoXDTIyMDQxODA2MzYzMVowgYsxCzAJBgNVBAYTAlpB
MQwwCgYDVQQIDANLWk4xEjAQBgNVBAcMCUhpbGxjcmVzdDERMA8GA1UECgwIV2Fy
ZXdvbGYxDzANBgNVBAsMBkRldk9wczEUMBIGA1UEAwwLb3Bzd29sZi5jb20xIDAe
BgkqhkiG9w0BCQEWEWFkbWluQG9wc3dvbGYuY29tMIIBIjANBgkqhkiG9w0BAQEF
AAOCAQ8AMIIBCgKCAQEA2eWOl6OjY/V6xPKYKC8NwrtOYfmr04KYR+5xuzZhNPXV
ICDZrHg3UfidSU9yiB8hRrZYlQ1YZw6kdfxYFiBqQV+450CHS2R9RbvPQTGxL0/I
lO4LQVodiTW7Khiemye0OId04Ak6yVz6wF+UScPb2HLRM7dW2OMbDpUcb/6QSCBK
1zdr6Co8O+okDdlXFSmqVuK5gIfT6lOKiny2XLaO6zPni4o6E5HzsX47YJiaTLCZ
J9X5oCWhB0wIVgX7vkdBxiwXACaHWlN32//wya1h1dQQpGUvttzEHl+wc0Fk6R9f
HKmP9owzuw40PPjdoOXhzqr7hCqszp/aTCqVFJU9xQIDAQABo1MwUTAdBgNVHQ4E
FgQUk+fn8dM59dkM0u6ZWnRp70TDupwwHwYDVR0jBBgwFoAUk+fn8dM59dkM0u6Z
WnRp70TDupwwDwYDVR0TAQH/BAUwAwEB/zANBgkqhkiG9w0BAQsFAAOCAQEAR05k
Ab9atURsGOHKZbKPFnwj6oKak3CcDSeB0wGAu75hKeGFBqisDg+s5pTcAlGgq8Md
fv6AzFtmskYeHqzt3TtZ091kLXGPrEf4Gv0zYdJ5kEi5RKIxNz57BnntlG/YA1FC
DAFen4U8zhavo4tQk04LkgnV4sHPutUMKqNNX64GAIfmeltr7yBaWs34nZ3+4OiF
c5/UqCGPmHgd2paDzQ3qc5tpCy86mY0zy7FreP/Z8VrnoOKIoH8ULjQAxiopl6zg
6bCLcDayKmfwBKrCgJobb76B7HJ5SKWpQCmgJeI/pFiQv67SsF63xtsPwtdmaY+T
SfOUJf/1oE9T9vp1yQ==
-----END CERTIFICATE-----
"@ | Out-File -LiteralPath "C:\cert.crt" -Encoding ascii -Force
	}
	if (!(Test-Path "C:\cert.key")) {
@"
-----BEGIN PRIVATE KEY-----
MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDZ5Y6Xo6Nj9XrE
8pgoLw3Cu05h+avTgphH7nG7NmE09dUgINmseDdR+J1JT3KIHyFGtliVDVhnDqR1
/FgWIGpBX7jnQIdLZH1Fu89BMbEvT8iU7gtBWh2JNbsqGJ6bJ7Q4h3TgCTrJXPrA
X5RJw9vYctEzt1bY4xsOlRxv/pBIIErXN2voKjw76iQN2VcVKapW4rmAh9PqU4qK
fLZcto7rM+eLijoTkfOxfjtgmJpMsJkn1fmgJaEHTAhWBfu+R0HGLBcAJodaU3fb
//DJrWHV1BCkZS+23MQeX7BzQWTpH18cqY/2jDO7DjQ8+N2g5eHOqvuEKqzOn9pM
KpUUlT3FAgMBAAECggEAVzFN8w4vRsOnggIVsxbJKeBsCDaxdGzw5O/coO6szVWG
GFos4KAmeu3CeuCI00GpvjMflV2Gv46TbwcwdII6IrjcM+WVfizTGEGEOPFalrUV
bcsnw9n8sbhHkhvR9AJaUriZo0DuPj+vs6VLoIz4f0/KuSgnX5jZbedrPsGeGM3e
HYGY/eCB1D6JzbDrW8jHe63SOPOizVA9m/c2CoH/YbL4rVN6+8aSAJaWnVzSUvPD
mRdY15EtF9VURU3C549Pw4C1RC0op2xvP8vlOFGsWDd2HHzxuo13UXd8NIes5zAE
VKIhLsEFkFIRwp7rTVaf9n6KCvvVuuG6N1Kxyv2roQKBgQD1Md/8BJIieFfd/fbq
zL+uAttBGuM88IUgx8c9usldWGYXvDOkmMQvkH7lnxFpOyZDynZyIw4ILrnh1T2+
f5g/qHxEabU59//aAbYoBAXxUUI7ZdBwzmn0yL6KU9hILDhRsEbVLOROC1tmUbUk
GIqTNMFBUimfy7LJJGTdxciouQKBgQDjf7kMijEbqP8bLUATdtlVoiOfuqOlMHYm
FzKsp75+rxSAjUyGvzBNe+HzSlfwPlD6bSYIm3Do80FVG+AawPN7uBApsTb32Lmj
nB1b1GUuN7VGQYJNxmrYe1m8VdLHN5kNM8hwOCpyYMdkFf6aYXAEtEO+iL9ghn1N
+tmW9e8fbQKBgQDIaFGIrTu8TNyUp4Vv+JYa5l7K4e0l2/kUB/YDsG3xi9U2RS94
sxx3PAVcLR2QAzaNZihVte08JuTrft2OnL+WGGIpkLT9goRubcOzBUbOLPqTje5G
pY/Y8VM7wLgglXQa4JekmaKpX4L/KH2D2UM6en4So9M9tsKUwNhoo8YUkQKBgQDQ
KQfrP28bvhBej5L3vGG0hz1NY/tkpOkWhVdqv7oANLbvwVpqWPobi+T9NeMtAfga
jFCmw4QWwq3e8DiogjDH3W18mJiRQ47o82mxorBKD9MgS8Ss4YbWOlerimPowSib
+evHMr00FvWa0L08CTf0NfVem8Vwzt5MweDiznlUKQKBgQCb/2zy03hepOHmr8oZ
2gUv8764Y865wFryfXoOlb+664sgMkNKJGzX/v97NQIeM4vFUb6FMVQO9z4pcXVq
w+jDPTUUugs8MOyE1bUNJutBgEjkeKN8bQt3mQlIhC6HSuwS+NHcku5sKobvjohj
SxVGgsgXs58fKq0k6khAOa4asQ==
-----END PRIVATE KEY-----
"@ | Out-File -LiteralPath "C:\cert.key" -Encoding ascii -Force
	}
	pip install pyftpdlib
	pip install 'cryptography==38.0.4'
	pip install 'pyOpenSSL==22.0.0'
	if (!(Test-Path "C:\ftps_entrypoint.py")) {
@"
import os, random, string

from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import TLS_FTPHandler
from pyftpdlib.servers import FTPServer

PASSIVE_PORTS = '56001-56008'

def main():
	user_dir = "C:/ftps_home/dev2"
	if not os.path.isdir(user_dir):
		os.mkdir(user_dir)
	authorizer = DummyAuthorizer()
	authorizer.add_user('dev2', 'Q/ulw&]', user_dir, perm="elradfmw")

	handler = TLS_FTPHandler
	handler.authorizer = authorizer
	handler.permit_foreign_addresses = True
	handler.certfile = 'C:/cert.crt'
	handler.keyfile = 'C:/cert.key'

	passive_ports = list(map(int, PASSIVE_PORTS.split('-')))
	handler.passive_ports = range(passive_ports[0], passive_ports[1])

	server = FTPServer(('0.0.0.0', 1010), handler)
	server.serve_forever()
	
if __name__ == '__main__':
	main()
"@ | Out-File -LiteralPath "C:\ftps_entrypoint.py" -Encoding utf8 -Force
	}
	pythonw -u "C:\ftps_entrypoint.py"
}
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
	Write-Error "This script expects to be run as Administrator. (Right click run as administrator)"
	exit 1
}
if ($ExcludeProjects.Length -eq 1 -and $ExcludeProjects[0].Contains(",")) {
    $SplitExcludeProjects = $ExcludeProjects[0]
    $ExcludeProjects = New-Object string[] $SplitExcludeProjects.Split(",").Count
    for ($j=0; $j -le $SplitExcludeProjects.Split(",").Count-1; $j++) {
        $ExcludeProjects[$j] = $SplitExcludeProjects.Split(",")[$j]
    }
}
if ($Projects.Length -eq 1 -and $Projects[0].Contains(",")) {
    $SplitProjects = $Projects[0]
    $Projects = New-Object string[] $SplitProjects.Split(",").Count
    for ($j=0; $j -le $SplitProjects.Split(",").Count-1; $j++) {
        $Projects[$j] = $SplitProjects.Split(",")[$j]
    }
}
if ($ExcludeCategories.Length -eq 1 -and $ExcludeCategories[0].Contains(",")) {
    $SplitExcludeCategories = $ExcludeCategories[0]
    $ExcludeCategories = New-Object string[] $SplitExcludeCategories.Split(",").Count
    for ($j=0; $j -le $SplitExcludeCategories.Split(",").Count-1; $j++) {
        $ExcludeCategories[$j] = $SplitExcludeCategories.Split(",")[$j]
    }
}
if ($Categories.Length -eq 1 -and $Categories[0].Contains(",")) {
    $SplitCategories = $Categories[0]
    $Categories = New-Object string[] $SplitCategories.Split(",").Count
    for ($j=0; $j -le $SplitCategories.Split(",").Count-1; $j++) {
        $Categories[$j] = $SplitCategories.Split(",")[$j]
    }
}
if ($PreTestRunScript -and $Coverage.IsPresent -and !($PreTestRunScript.Contains("-Coverage"))) {
	$PreTestRunScript += " -Coverage"
}
if ($PostTestRunScript -and $Coverage.IsPresent -and !($PostTestRunScript.Contains("-Coverage"))) {
	$PostTestRunScript += " -Coverage"
}
$TestResultsPath = ".\TestResults"
if (Test-Path "$TestResultsPath") {
	Remove-Item -Force -Recurse "$TestResultsPath"
}
if ($VSTestPath -eq $null -or $VSTestPath -eq "" -or !(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
	$VSTestPath = ".\Microsoft.TestPlatform\tools\net451\common7\ide"
} else {
	if ($InContainer.IsPresent -or $InContainerCommitID -ne "latest" -or $InContainerVersion -ne "latest") {
		Write-Warning -Message "Ignoring VSTestPath parameter because it cannot be used with the -InContainer or -ContainerID parameters."
		$VSTestPath = ".\Microsoft.TestPlatform\tools\net451\Common7\IDE"
	}
}
if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
	#Find NuGet
	if ("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) {
		$NuGetCommand = Get-Command NuGet -ErrorAction SilentlyContinue
		if ($NuGetCommand) {
			$NuGet = $NuGetCommand.Path
		}
	}
	if (("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) -and (Test-Path "$env:windir")) {
		wget "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "$env:windir\nuget.exe"
		$NuGet = "$env:windir\nuget.exe"
	}
	if ("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) {
		Write-Host NuGet not found. Download from: https://dist.nuget.org/win-x86-commandline/latest/nuget.exe to a directory in the PATH environment variable like c:\windows\nuget.exe. Or use the -NuGet switch.
		sleep 10
		exit 1
	}
}
if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
	&"nuget.exe" "install" "Microsoft.TestPlatform" "-ExcludeVersion" "-NonInteractive" "-OutputDirectory" "."
	if (!(Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
		Write-Error "Cannot install test runner using nuget."
		exit 1
	}
}
if ($Coverage.IsPresent) {
	Write-Host Removing existing Coverage Reports
	if (Test-Path "$TestResultsPath\Merged.coveragexml") {
		Remove-Item "$TestResultsPath\Merged.coveragexml"
	}
	if (Test-Path "$TestResultsPath\Cobertura.xml") {
		Remove-Item "$TestResultsPath\Cobertura.xml"
	}
	$CoverageConfigPath = ".\Microsoft.TestPlatform\tools\net451\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.config"
	(Get-Content $CoverageConfigPath).replace('<UseVerifiableInstrumentation>true</UseVerifiableInstrumentation>', '<UseVerifiableInstrumentation>false</UseVerifiableInstrumentation>') | Set-Content $CoverageConfigPath
}
if ($CreateLocalSchedulerAdmin.IsPresent) {
	cmd /c NET user "LocalSchedulerAdmin" "987Sched#@!" /ADD /Y
	Add-LocalGroupMember -Group 'Administrators' -Member ('LocalSchedulerAdmin') -Verbose
	Add-LocalGroupMember -Group 'Warewolf Administrators' -Member ('LocalSchedulerAdmin') -Verbose
}
if ($UseRegionalSettings.IsPresent) {
	$culture = [System.Globalization.CultureInfo]::CreateSpecificCulture("en-ZA")      
    $assembly = [System.Reflection.Assembly]::Load("System.Management.Automation")
    $type = $assembly.GetType("Microsoft.PowerShell.NativeCultureResolver")
    $field = $type.GetField("m_uiCulture", [Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Static)
    $field.SetValue($null, $culture)      
    Set-Culture en-ZA
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sTimeFormat -Value 'hh:mm:ss tt' } }
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sShortTime -Value 'hh:mm tt' } }
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sLongDate -Value 'dddd, dd MMMM yyyy' } }
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sShortDate -Value 'yyyy/MM/dd' } }
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sDecimal -Value '.' } }
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name s1159 -Value 'AM' } }
    Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name s2359 -Value 'PM' } }
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sTimeFormat -Value 'hh:mm:ss tt'
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sShortTime -Value 'hh:mm tt'
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sLongDate -Value 'dddd, dd MMMM yyyy'
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sShortDate -Value 'yyyy/MM/dd'
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sDecimal -Value '.'
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name s1159 -Value 'AM'
    Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name s2359 -Value 'PM'
}
if ($CreateUNCPath.IsPresent) {	
    mkdir C:\FileSystemShareTestingSite\ReadFileSharedTestingSite
    "file contents to read" | Out-File -LiteralPath "C:\FileSystemShareTestingSite\ReadFileSharedTestingSite\filetoread.txt" -Encoding utf8 -Force
    New-SmbShare -Path C:\FileSystemShareTestingSite -FullAccess Everyone -Name FileSystemShareTestingSite
}
if ($STA.IsPresent) {
	if (!(Test-Path "$TestResultsPath")) {
		mkdir "$TestResultsPath"
	}
@"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>
  </RunConfiguration>
</RunSettings>
"@ | Out-File -LiteralPath "$TestResultsPath\STA.runsettings" -Encoding utf8 -Force
}
if ($Projects.Length -gt 0) {
	for ($LoopCounter=0; $LoopCounter -le $RetryCount; $LoopCounter++) {
		if ($StartFTPServer.IsPresent) {
			Start-FTPServer
		}
		if ($StartFTPSServer.IsPresent) {
			Start-FTPSServer
		}
		if ($RetryRebuild.IsPresent) {
			if (Test-Path "$PWD\..\..\Compile.ps1") {
				&"$PWD\..\..\Compile.ps1" "-AcceptanceTesting -NuGet `"$NuGet`" -MSBuildPath `"$MSBuildPath`""
			} else {
				if (Test-Path "$PWD\Compile.ps1") {
					&"$PWD\Compile.ps1" "-AcceptanceTesting -NuGet `"$NuGet`" -MSBuildPath `"$MSBuildPath`""
					Set-Location "$PWD\bin\AcceptanceTesting"
				}
			}
		} else {
			if (!(Test-Path "$PWD\*tests.dll") -and $InContainerCommitID -eq "latest") {
				Write-Error "This script expects to be run from a directory containing test assemblies. (Files with names that end in tests.dll)"
				exit 1
			}
		}
		$AllAssemblies = @()
		foreach ($project in $Projects) {
			$AllAssemblies += @(Get-ChildItem ".\$project.dll" -Recurse)
		}
		if ($AllAssemblies.Count -le 0) {
			$ShowError = "Could not find any assemblies in the current environment directory matching the project definition of: " + ($Projects -join ",")
			Write-Error $ShowError
		}
		$AssembliesList = @()
		for ($i = 0; $i -lt $AllAssemblies.Count; $i++) 
		{
			if ([array]::indexof($ExcludeProjects, $AllAssemblies[$i].Name.TrimEnd(".dll")) -eq -1) {
				$AssembliesList += @($AllAssemblies[$i].Name)
			}
		}
		if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
			Remove-Item "$VSTestPath\Extensions\TestPlatform\TestResults" -Force -Recurse
		}
		New-Item -ItemType Directory "$TestResultsPath" -ErrorAction SilentlyContinue
		if (Test-Path "$TestResultsPath\RunTests.ps1") {
			Move-Item "$TestResultsPath\RunTests.ps1" "$TestResultsPath\RunTests($LoopCounter).ps1"
		}
		if (Test-Path "$TestResultsPath\warewolf-server.log") {
			Move-Item "$TestResultsPath\warewolf-server.log" "$TestResultsPath\warewolf-server($LoopCounter).ps1"
		}
		if (Test-Path "$TestResultsPath\Snapshot.coverage") {
			Move-Item "$TestResultsPath\Snapshot.coverage" "$TestResultsPath\Snapshot($LoopCounter).coverage"
		}
		if (Test-Path "$TestResultsPath\Snapshot_Backup.coverage") {
			Move-Item "$TestResultsPath\Snapshot_Backup.coverage" "$TestResultsPath\Snapshot_Backup($LoopCounter).coverage"
		}
		$AssembliesArg = ".\" + ($AssembliesList -join " .\")
		if ($UNCPassword) {
			"net use \\DEVOPSPDC.premier.local\FileSystemShareTestingSite /user:Administrator $UNCPassword" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		}
		if ($STA.IsPresent) {
			$STAArg = "--settings:`"$TestResultsPath\STA.runsettings`""
		} else {
			$STAArg = ""
		}
		if ($TestsToRun) {
			if ($PreTestRunScript) {
				"&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx /platform:x64 $AssembliesArg /Tests:`"$TestsToRun`" $STAArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			} else {
				if ($Coverage.IsPresent -and !($PreTestRunScript)) {
					"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx /platform:x64 $AssembliesArg /Tests:`"$TestsToRun`" $STAArg /EnableCodeCoverage" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
				} else {
					"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx /platform:x64 $AssembliesArg /Tests:`"$TestsToRun`" $STAArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
				}
			}
		} else {
			$CategoryArg = ""
			if ($ExcludeCategories -ne $null -and $ExcludeCategories -ne @()) {
				if ($ExcludeCategories.Count -eq 1 -and $ExcludeCategories[0].Contains(",")) {
					$ExcludeCategories = $ExcludeCategories[0] -split ","
				}
				$CategoryArg = "/TestCaseFilter:`"(TestCategory!="
				$CategoryArg += $ExcludeCategories -join ")&(TestCategory!="
				$CategoryArg += ")`""
			} else {
				if ($Category -ne $null -and $Category -ne "") {
					$CategoryArg = "/TestCaseFilter:`"(TestCategory=" + $Category + ")`""
				} else {
					if ($Categories -ne $null -and $Categories.Count -ne 0) {
						$CategoryArg = "/TestCaseFilter:`"(TestCategory="
						$CategoryArg += $Categories -join ")|(TestCategory="
						$CategoryArg += ")`""
					}
				}
			}
			if ($PreTestRunScript) {
				"&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
				"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx /platform:x64 $AssembliesArg $CategoryArg $STAArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
			} else {
				if ($Coverage.IsPresent -and !($PreTestRunScript)) {
					"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx /platform:x64 $AssembliesArg $CategoryArg $STAArg /EnableCodeCoverage" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
				} else {
					"&`"$VSTestPath\Extensions\TestPlatform\vstest.console.exe`" /logger:trx /platform:x64 $AssembliesArg $CategoryArg $STAArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
				}
			}
		}
		if ($PostTestRunScript) {
			"&.\$PostTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		}
		if ($UNCPassword) {
			"net use \\DEVOPSPDC.premier.local\FileSystemShareTestingSite /delete" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
		}
		Get-Content "$TestResultsPath\RunTests.ps1"
		if (!($InContainer.IsPresent) -and $InContainerCommitID -eq "latest" -and $InContainerVersion -eq "latest") {
			&"$TestResultsPath\RunTests.ps1"
		} else {
			if ($InContainerCommitID -eq "latest") {
				docker run -i --rm --memory 4g -v "${PWD}:C:\BuildUnderTest" registry.gitlab.com/warewolf/vstest:$InContainerVersion powershell -Command Set-Location .\BuildUnderTest`;`&.\TestResults\RunTests.ps1
			} else {
				docker run -i --rm --memory 4g -v "${PWD}\TestResults:C:\BuildUnderTest\TestResults" registry.gitlab.com/warewolf/vstest:$InContainerCommitID powershell -Command Set-Location .\BuildUnderTest`;`&.\TestResults\RunTests.ps1
			}
		}
		if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
			Copy-Item "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx" "$TestResultsPath" -Force -Recurse
		}
		if (Test-Path "$TestResultsPath\*.trx") {
			[System.Collections.ArrayList]$getXMLFiles = @(Get-ChildItem "$TestResultsPath\*.trx")
			if ($getXMLFiles.Count -gt 1) {
				$MaxCount = 0
				$MaxCountIndex = 0
				$CountIndex = 0
				$getXMLFiles | % {
					$getXML = [xml](Get-Content $_.FullName)
					if ([int]$getXML.TestRun.ResultSummary.Counters.total -gt $MaxCount) {
						$MaxCount = $getXML.TestRun.ResultSummary.Counters.total
						$MaxCountIndex = $CountIndex
					}
					$CountIndex++
				}
				$BaseXMLFile = $getXMLFiles[$MaxCountIndex]
				$getBaseXML = [xml](Get-Content $BaseXMLFile.FullName)
				$getXMLFiles.RemoveAt($MaxCountIndex)
				$getXMLFiles | % {
					$getXML = [xml](Get-Content $_.FullName)
					$getXMl.TestRun.Results.UnitTestResult | % {
						$RetryUnitTestResult = $_
						$getBaseXMl.TestRun.Results.UnitTestResult | % {
							if ($RetryUnitTestResult.testName -eq $_.testName -and $RetryUnitTestResult.outcome -eq 'Passed' -and $_.outcome -eq 'Failed') {
								[void]$_.ParentNode.AppendChild($getBaseXMl.ImportNode($RetryUnitTestResult, $true))
								$_.ParentNode.ParentNode.ResultSummary.Counters.SetAttribute("passed", [int]($_.ParentNode.ParentNode.ResultSummary.Counters.passed) + 1)
								$_.ParentNode.ParentNode.ResultSummary.Counters.SetAttribute("failed", [int]($_.ParentNode.ParentNode.ResultSummary.Counters.failed) - 1)
								[void]$_.ParentNode.RemoveChild($_)
							}
						}
					}
					Remove-Item $_.FullName
				}
				$getBaseXML.Save($BaseXMLFile.FullName)
			} else {
				$getBaseXML = [xml](Get-Content $getXMLFiles[0].FullName)
			}
			if ($getBaseXML.TestRun.ResultSummary.Counters.passed -ne $getBaseXML.TestRun.ResultSummary.Counters.executed) {
				$TestsToRun = ($getBaseXML.TestRun.Results.UnitTestResult | Where-Object {$_.outcome -ne "Passed"}).testName -join ","
			} else {
				break
			}
		} else {
			Write-Error "No test results found."
			exit 1
		}
		if ($StartFTPServer.IsPresent -or $StartFTPSServer.IsPresent) {
			taskkill /im pythonw.exe /f
			taskkill /im pythonw3.10.exe /f
		}
	}
} else {
	if ($StartFTPServer.IsPresent) {
		Start-FTPServer
	}
	if ($StartFTPSServer.IsPresent) {
		Start-FTPSServer
	}
}
if ($Coverage.IsPresent) {
	$MergedSnapshotPath = "$TestResultsPath\Merged.coveragexml"
	$CoverageToolPath = ".\Microsoft.TestPlatform\tools\net451\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe"
	$GetSnapshots = Get-ChildItem "$TestResultsPath\**\*.coverage"
	if ($GetSnapshots.count -le 0) {
		$GetSnapshots = Get-ChildItem "$TestResultsPath\*.coverage"
	}
	if ($GetSnapshots.count -le 0) {
		Write-Host Cannot find snapshots in $TestResultsPath
		exit 1
	}
	Write-Host `&`"$CoverageToolPath`" analyze /output:`"$MergedSnapshotPath`" @GetSnapshots
	&"$CoverageToolPath" analyze /output:"$MergedSnapshotPath" @GetSnapshots
	$reportGeneratorExecutable = ".\reportgenerator.exe"

    if (!(Test-Path "$reportGeneratorExecutable")) {
        dotnet tool install dotnet-reportgenerator-globaltool --tool-path "."
    }
    
    $reportGeneratorCoberturaParams = @(
        "-reports:$MergedSnapshotPath",
        "-targetdir:$TestResultsPath",
        "-reporttypes:Cobertura"
    )
    
    Write-Output "Executing Report Generator with following parameters: $reportGeneratorCoberturaParams."
    &"$reportGeneratorExecutable" @reportGeneratorCoberturaParams
}
exit 0
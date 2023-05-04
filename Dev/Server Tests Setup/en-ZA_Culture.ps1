$culture = [System.Globalization.CultureInfo]::CreateSpecificCulture("en-ZA")

$assembly = [System.Reflection.Assembly]::Load("System.Management.Automation")
$type = $assembly.GetType("Microsoft.PowerShell.NativeCultureResolver")
$field = $type.GetField("m_uiCulture", [Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Static)
$field.SetValue($null, $culture)

Set-Culture en-ZA
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sTimeFormat -Value 'hh:mm:ss tt' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sTimeFormat -Value 'hh:mm:ss tt' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sShortTime -Value 'hh:mm tt' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sLongDate -Value 'dddd, dd MMMM yyyy' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sShortDate -Value 'yyyy/MM/dd' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sDecimal -Value '.' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name s1159 -Value 'AM' } }
Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if ($SubKeyName -ne 'HKEY_USERS\S-1-5-93-2-1_Classes') { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name s2359 -Value 'PM' } }
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sTimeFormat -Value 'hh:mm:ss tt'
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sShortTime -Value 'hh:mm tt'
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sLongDate -Value 'dddd, dd MMMM yyyy'
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sShortDate -Value 'yyyy/MM/dd'
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sDecimal -Value '.'
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name s1159 -Value 'AM'
Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name s2359 -Value 'PM'
﻿using System.Diagnostics;
using TechTalk.SpecFlow;
using Microsoft.Win32;

namespace Warewolf.Tools.Specs
{
    [Binding]
    class RegionalSettingsSteps
    {
        [Given(@"The system short date format is ""(.*)"" and the long time format is ""(.*)""")]
        public void GivenTheSystemShortDateFormatIs(string p0, string p1)
        {
            var alreadySet = true;
            if (GetRegistryEntry(Registry.Users, @".DEFAULT\Control Panel\International", "sShortDate") != p0)
            {
                alreadySet = false;
            }
            if (GetRegistryEntry(Registry.CurrentUser, @"Control Panel\International", "sShortDate") != p0)
            {
                alreadySet = false;
            }
            if (GetRegistryEntry(Registry.Users, @".DEFAULT\Control Panel\International", "sTimeFormat") != p1)
            {
                alreadySet = false;
            }
            if (GetRegistryEntry(Registry.CurrentUser, @"Control Panel\International", "sTimeFormat") != p1)
            {
                alreadySet = false;
            }
            if (!alreadySet)
            {
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_USERS\.DEFAULT\Control Panel\International", "sShortDate", p0);
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International", "sShortDate", p0);
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_USERS\.DEFAULT\Control Panel\International", "sTimeFormat", p1);
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International", "sTimeFormat", p1);
            }
        }

        string GetRegistryEntry(RegistryKey root, string path, string name)
        {
            using (RegistryKey key = root.OpenSubKey(path))
            {
                return key.GetValue(name) as string;
            }
        }

        static void ChangeRegistryEntry(string entryPath, string entryName, string newValue) => ExecutePowershellCommand($"Set-ItemProperty -Path '{entryPath}' -Name {entryName} -Value '{newValue}'");

        static void ExecutePowershellCommand(string command)
        {
            var process = new Process();

            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                ErrorDialog = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -Command \"{command}\""
            };

            process.Start();
            process.WaitForExit();
        }
    }
}

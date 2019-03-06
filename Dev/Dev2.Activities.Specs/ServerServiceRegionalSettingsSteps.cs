using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TechTalk.SpecFlow;
using Microsoft.Win32;
using Warewolf.Tools.Specs;

namespace Dev2.Activities.Specs
{
    [Binding]
    class ServerServiceRegionalSettingsSteps
    {
        [Given(@"The system short date format is ""(.*)"" and the long time format is ""(.*)""")]
        public void GivenTheSystemShortDateFormatIs(string p0, string p1)
        {
            var alreadySet = true;
            if (RegionalSettingsSteps.GetRegistryEntry(Registry.Users, @".DEFAULT\Control Panel\International", "sShortDate") != p0)
            {
                alreadySet = false;
            }
            if (RegionalSettingsSteps.GetRegistryEntry(Registry.CurrentUser, @"Control Panel\International", "sShortDate") != p0)
            {
                alreadySet = false;
            }
            if (RegionalSettingsSteps.GetRegistryEntry(Registry.Users, @".DEFAULT\Control Panel\International", "sTimeFormat") != p1)
            {
                alreadySet = false;
            }
            if (RegionalSettingsSteps.GetRegistryEntry(Registry.CurrentUser, @"Control Panel\International", "sTimeFormat") != p1)
            {
                alreadySet = false;
            }
            if (!alreadySet)
            {
                var serverStartedFilePath = RegionalSettingsSteps.GetRegistryEntry(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Warewolf Server", "ImagePath").Replace("Warewolf Server.exe", "ServerStarted");
                StopServerService();
                RegionalSettingsSteps.ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_USERS\.DEFAULT\Control Panel\International", "sShortDate", p0);
                RegionalSettingsSteps.ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International", "sShortDate", p0);
                RegionalSettingsSteps.ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_USERS\.DEFAULT\Control Panel\International", "sTimeFormat", p1);
                RegionalSettingsSteps.ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International", "sTimeFormat", p1);
                StartServerService(serverStartedFilePath);
            }
        }

        void StartServerService(string serverStartedFilePath)
        {
            File.Delete(serverStartedFilePath);
            RegionalSettingsSteps.ExecutePowershellCommand("sc.exe start 'Warewolf Server'");
            var exists = false;
            var RetryCount = 0;
            while (!exists && RetryCount < 150)
            {
                RetryCount++;
                if (File.Exists(serverStartedFilePath))
                {
                    exists = true;
                }
                else
                {
                    Console.WriteLine($"Still waiting for {serverStartedFilePath} file to exist.");
                    Thread.Sleep(3000);
                }
            }
            if (!exists)
            {
                throw new Exception("Server cannot start after making regional settings changes.");
            }
        }

        void StopServerService() => RegionalSettingsSteps.ExecutePowershellCommand("sc.exe stop 'Warewolf Server'");
    }
}

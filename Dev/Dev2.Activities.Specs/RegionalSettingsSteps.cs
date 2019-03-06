using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TechTalk.SpecFlow;
using Microsoft.Win32;

namespace Dev2.Activities.Specs
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
                var serverStartedFilePath = GetRegistryEntry(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Warewolf Server", "ImagePath").Replace("Warewolf Server.exe", "ServerStarted");
                StopServerService();
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_USERS\.DEFAULT\Control Panel\International", "sShortDate", p0);
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International", "sShortDate", p0);
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_USERS\.DEFAULT\Control Panel\International", "sTimeFormat", p1);
                ChangeRegistryEntry(@"Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International", "sTimeFormat", p1);
                StartServerService(serverStartedFilePath);
            }
        }

        string GetRegistryEntry(RegistryKey root, string path, string name)
        {
            using (RegistryKey key = root.OpenSubKey(path))
            {
                return key.GetValue(name) as string;
            }
        }

        void StartServerService(string serverStartedFilePath)
        {
            File.Delete(serverStartedFilePath);
            ExecutePowershellCommand("sc.exe start 'Warewolf Server'");
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

        void StopServerService() => ExecutePowershellCommand("sc.exe stop 'Warewolf Server'");

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

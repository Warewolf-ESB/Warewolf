using Dev2.Common;
using Dev2.Common.Interfaces;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Dev2.Factory
{
    public static class CustomGitOps
    {

        public static void SetCustomGitTool(IExternalProcessExecutor processExecutor)
        {
            var secondCommand = "config --global mergetool.DiffMerge.cmd \"C:/Program Files (x86)/Warewolf/Studio/customMerge.sh \"-merge\" $REMOTE";
            var secondCommand1 = "config --global difftool.DiffMerge.cmd \"C:/Program Files (x86)/Warewolf/Studio/customMerge.sh \"-merge\" $REMOTE";
            var orderedList1 = new[]
            {
                "config --global diff.tool DiffMerge"
                ,secondCommand
                ,"config --global mergetool.DiffMerge.trustExitCode false"
                ,"config --global diff.tool DiffMerge"
                ,secondCommand1
                ,"config --global difftool.DiffMerge.trustExitCode false"

            };

            var gitExePath = GetGitBashPath();

            if (!string.IsNullOrEmpty(gitExePath))
            {
                foreach (var item in orderedList1)
                {
                    try
                    {
                        ProcessStartInfo ProcessInfo;
                        Process Process;

                        string arguments = "/C " + "\"" + gitExePath + "\"" + " " + item;
                        ProcessInfo = new ProcessStartInfo("cmd.exe", arguments)
                        {
                            CreateNoWindow = true,  
                            UseShellExecute = false
                        };
                        Process = processExecutor.Start(ProcessInfo);
                        if (Process != null)
                        {
                            Process.WaitForExit(10);
                            Process.Close();
                            Process.Dispose();
                        }

                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Error(ex.Message, "Git Setup error");
                    }
                }
            }
        }
        static string GetGitBashPath()
        {
            var location = ConfigurationManager.AppSettings["GitRegistryKey"];
            using (var a = Registry.LocalMachine.OpenSubKey(location))
            {
                if (a != null)
                {
                    var path = a.GetValue("");
                    var myPath = string.Copy(path.ToString());
                    var space = myPath.LastIndexOf(".exe", StringComparison.InvariantCultureIgnoreCase);
                    var cleanPath = myPath.Substring(1, space + 3);

                    var gitParrentFolder = Path.GetDirectoryName(cleanPath);
                    var binFolder = Directory.GetDirectories(gitParrentFolder).ToList().SingleOrDefault(p => p.EndsWith("bin", StringComparison.InvariantCultureIgnoreCase));
                    var gitExe = Directory.GetFiles(binFolder).ToList().SingleOrDefault(p => p.EndsWith("git.exe", StringComparison.InvariantCultureIgnoreCase));
                    return gitExe;
                }

                return "";
            }
        }
    }
}

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
                "config --global merge.tool DiffMerge"
                ,secondCommand
                ,"config --global mergetool.DiffMerge.trustExitCode false"

                ,"config --global diff.tool DiffMerge"
                ,secondCommand1
                ,"config --global difftool.DiffMerge.trustExitCode false"

            };

            var gitExePath = GetGitExePath(processExecutor);

            if (!string.IsNullOrEmpty(gitExePath))
            {
                foreach (var item in orderedList1)
                {
                    try
                    {
                        var quotedExePath = "\"" + gitExePath + "\"";
                        ProcessStartInfo ProcessInfo = new ProcessStartInfo(quotedExePath, item)
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        Process Process = processExecutor.Start(ProcessInfo);
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
        static string GetGitExePath(IExternalProcessExecutor processExecutor)
        {
            try
            {
                ProcessStartInfo ProcessInfo = new ProcessStartInfo("cmd.exe", "/C where.exe git.exe")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                Process Process = processExecutor.Start(ProcessInfo);
                using (Process)
                {
                    Process.WaitForExit(10);
                    using (StreamReader myOutput = Process.StandardOutput)
                    {
                        if (Process.HasExited)
                        {
                            string pathToGitExe = myOutput.ReadToEnd();
                            var gitpath = pathToGitExe.Split(Environment.NewLine.ToArray())
                                .FirstOrDefault(p => p.EndsWith(@"bin\git.exe", StringComparison.InvariantCultureIgnoreCase));
                            return gitpath;
                        }

                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, "Git Setup error");
                return "";
            }
        }
    }
}

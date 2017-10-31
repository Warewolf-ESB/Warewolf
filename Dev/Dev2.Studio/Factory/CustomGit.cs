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

            var gitExePath = GetGitExePath();
            if (string.IsNullOrEmpty(gitExePath))
            {
                gitExePath = "git.exe";
            }
                foreach (var item in orderedList1)
                {
                    try
                    {
                        var quotedExePath = "\"" + gitExePath + "\"";

                        ExecuteCommand(processExecutor, quotedExePath, item);

                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Error(ex.Message, "Git Setup error");
                    }
                }
        }

        private static string ExecuteCommand(IExternalProcessExecutor processExecutor, string exe, string command)
        {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo(exe, command);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                Process proc = processExecutor.Start(procStartInfo);
                string result = proc.StandardOutput.ReadToEnd();
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }
        static string GetGitExePath()
        {
            try
            {
                var args = "/c where.exe git.exe";
                var exe = "cmd.exe ";
                var procf = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                procf.Start();
                string gitLocation = procf.StandardOutput.ReadLine();
                var correctPath = gitLocation.Split(Environment.NewLine.ToCharArray())
                    .FirstOrDefault(p => p.EndsWith(@"bin\git.exe", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(@"git.exe", StringComparison.InvariantCultureIgnoreCase));
                return correctPath;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, "Git Setup error");
                return "";
            }
        }
    }
}

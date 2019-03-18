#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using Dev2.Common;
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
            var secondCommand = "config --global mergetool.DiffMerge.cmd \"'C:/Program Files (x86)/Warewolf/Studio/MergePowershellScript/customMerge.sh' $REMOTE";
            var secondCommand1 = "config --global difftool.DiffMerge.cmd \"'C:/Program Files (x86)/Warewolf/Studio/MergePowershellScript/customMerge.sh' $REMOTE";
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
                var procStartInfo = new ProcessStartInfo(exe, command);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                var proc = processExecutor.Start(procStartInfo);
                var result = proc.StandardOutput.ReadToEnd();
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
                var gitLocation = procf.StandardOutput.ReadLine();
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

using System;
using System.Diagnostics;

namespace Warewolf.Launcher
{
    static class ProcessUtils
    {
        public static Process StartProcess(string exeName, string args)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = exeName;
            process.StartInfo.Arguments = args;
            process.Start();
            process.WaitForExit();
            return process;
        }

        public static string RunFileInThisProcess(string TestRunnerPath, string args = "")
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = TestRunnerPath;
            Process process = new Process();
            process.StartInfo = startinfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.Arguments = args;
            process.Start();
            var trxFilePath = "";

            while (!process.StandardOutput.EndOfStream)
            {
                string testRunLine = process.StandardOutput.ReadLine();
                Console.WriteLine(testRunLine);
                if (testRunLine.StartsWith("Results File: "))
                {
                    trxFilePath = ParseTrxFilePath(testRunLine);
                }
            }
            process.WaitForExit();
            string allErrors = process.StandardError.ReadToEnd();
            Console.WriteLine(allErrors);
            return trxFilePath;
        }

        static string ParseTrxFilePath(string standardOutput)
        {
            const string parseFrom = "Results File: ";
            int StartIndex = standardOutput.IndexOf(parseFrom) + parseFrom.Length;
            return standardOutput.Substring(StartIndex, standardOutput.Length - StartIndex);
        }
    }
}

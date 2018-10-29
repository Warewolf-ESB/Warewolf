using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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

        static string TRXPath;

        public static string RunFileInThisProcess(string TestRunnerPath, string args = "", string logFilePath="")
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = TestRunnerPath;
            Process process = new Process();
            process.StartInfo = startinfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.Arguments = args;
            TRXPath = null;
            process.OutputDataReceived += (sender, arguments) => Display(arguments.Data, logFilePath);
            process.ErrorDataReceived += (sender, arguments) => Display(arguments.Data, logFilePath);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.WaitForExit();
            return TRXPath;
        }

        static void Display(string output, string logFilePath = "")
        {
            if (string.IsNullOrEmpty(logFilePath))
            {
                Console.WriteLine(output);
            }
            else
            {
                File.AppendAllLines(logFilePath, new string[] { output }, System.Text.Encoding.UTF8);
            }
            if (output != null && output.StartsWith("Results File: "))
            {
                TRXPath = ParseTrxFilePath(output);
            }
        }

        static string ParseTrxFilePath(string standardOutput)
        {
            const string parseFrom = "Results File: ";
            int StartIndex = standardOutput.IndexOf(parseFrom) + parseFrom.Length;
            return standardOutput.Substring(StartIndex, standardOutput.Length - StartIndex);
        }
    }
}

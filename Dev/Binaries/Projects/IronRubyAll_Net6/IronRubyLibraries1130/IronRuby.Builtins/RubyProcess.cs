using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using IronRuby.Runtime;
using Microsoft.Scripting;

namespace IronRuby.Builtins
{
	[RubyModule("Process", BuildConfig = "!SILVERLIGHT")]
	public static class RubyProcess
	{
		[HideMethod("new", IsStatic = true)]
		[RubyClass("Status", BuildConfig = "!SILVERLIGHT")]
		public sealed class Status
		{
			private readonly Process _process;

			internal Status(Process process)
			{
				_process = process;
			}

			[RubyMethod("coredump?")]
			public static bool CoreDump(Status self)
			{
				return false;
			}

			[RubyMethod("exitstatus")]
			public static int ExitStatus(Status self)
			{
				return self._process.ExitCode;
			}

			[RubyMethod("exited?")]
			public static bool Exited(Status self)
			{
				return self._process.HasExited;
			}

			[RubyMethod("pid")]
			public static int Pid(Status self)
			{
				return self._process.Id;
			}

			[RubyMethod("stopped?")]
			public static bool Stopped(Status self)
			{
				return false;
			}

			[RubyMethod("stopsig")]
			public static object StopSig(Status self)
			{
				return null;
			}

			[RubyMethod("success?")]
			public static bool Success(Status self)
			{
				return self._process.ExitCode == 0;
			}

			[RubyMethod("termsig")]
			public static object TermSig(Status self)
			{
				return null;
			}

			[RubyMethod("inspect")]
			public static MutableString Inspect(Status self)
			{
				return MutableString.CreateAscii(string.Format(CultureInfo.InvariantCulture, "#<Process::Status: pid={0},{1}({2})>", new object[3]
				{
					Pid(self),
					Exited(self) ? "exited" : "running",
					ExitStatus(self)
				}));
			}
		}

		private static string[] _ExecutableExtensions = new string[4] { ".exe", ".com", ".bat", ".cmd" };

		internal static Process CreateProcess(RubyContext context, MutableString command, MutableString[] args)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo(command.ToString(), JoinArguments(args).ToString());
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardError = true;
			try
			{
				Process process = Process.Start(processStartInfo);
				process.WaitForExit();
				context.ChildProcessExitStatus = new Status(process);
				return process;
			}
			catch (Exception ex)
			{
				throw RubyExceptions.CreateENOENT(processStartInfo.FileName, ex);
			}
		}

		internal static Process CreateProcess(RubyContext context, MutableString command, bool redirectOutput)
		{
			return CreateProcess(context, command, false, redirectOutput, false);
		}

		internal static Process CreateProcess(RubyContext context, MutableString command, bool redirectInput, bool redirectOutput, bool redirectErrorOutput)
		{
			string executable;
			string arguments;
			GetExecutable(context.DomainManager.Platform, command.ToString(), out executable, out arguments);
			Process process = new Process();
			process.StartInfo.FileName = executable;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardInput = redirectInput;
			process.StartInfo.RedirectStandardOutput = redirectOutput;
			process.StartInfo.RedirectStandardError = redirectErrorOutput;
			try
			{
				process.Start();
			}
			catch (Exception ex)
			{
				throw RubyExceptions.CreateENOENT(executable, ex);
			}
			context.ChildProcessExitStatus = new Status(process);
			return process;
		}

		private static void GetExecutable(PlatformAdaptationLayer pal, string command, out string executable, out string arguments)
		{
			command = command.Trim(' ');
			if (command.Length == 0)
			{
				throw RubyExceptions.CreateEINVAL(command);
			}
			string text = pal.GetEnvironmentVariable("COMSPEC");
			if (!pal.FileExists(text))
			{
				text = null;
			}
			if (text != null && IndexOfUnquotedSpecialCharacter(command) >= 0)
			{
				executable = text;
				arguments = "/c \"" + command + "\"";
				return;
			}
			int i = 0;
			while (true)
			{
				int num = command.IndexOf(' ', i);
				executable = ((num >= 0) ? command.Substring(0, num) : command);
				arguments = ((num >= 0) ? command.Substring(num + 1) : "");
				if (i == 0 && text != null && IsShellCommand(executable))
				{
					executable = text;
					arguments = "/c \"" + command + "\"";
					return;
				}
				try
				{
					foreach (string executableFile in GetExecutableFiles(pal, executable))
					{
						if (pal.FileExists(executableFile))
						{
							executable = executableFile;
							return;
						}
					}
				}
				catch (Exception ex)
				{
					if (num < 0)
					{
						throw RubyExceptions.CreateENOENT(command, ex);
					}
				}
				if (num < 0)
				{
					break;
				}
				for (i = num + 1; i < command.Length && command[i] == ' '; i++)
				{
				}
			}
			throw RubyExceptions.CreateENOENT(command);
		}

		private static int IndexOfUnquotedSpecialCharacter(string str)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < str.Length; i++)
			{
				switch (str[i])
				{
				case '"':
					flag = !flag;
					break;
				case '\'':
					flag2 = !flag2;
					break;
				case '<':
				case '>':
				case '|':
					if (!flag2 && !flag)
					{
						return i;
					}
					break;
				}
			}
			return -1;
		}

        private static IEnumerable<string>/*!*/ GetExecutableFiles(PlatformAdaptationLayer/*!*/ pal, string/*!*/ path)
        {
            if (path[0] == '"' || path[path.Length - 1] == '"')
            {
                if (path.Length >= 3 && path[0] == '"' && path[path.Length - 1] == '"')
                {
                    path = path.Substring(1, path.Length - 2);
                }
                else
                {
                    yield break;
                }
            }

            string extension = RubyUtils.GetExtension(path);
            bool hasExtension = !String.IsNullOrEmpty(extension);
            bool isExecutable = hasExtension && Array.IndexOf(_ExecutableExtensions, extension.ToLowerInvariant()) >= 0;

            if (!hasExtension || isExecutable)
            {
                foreach (var fullPath in GetAbsolutePaths(pal, path))
                {
                    if (hasExtension)
                    {
                        yield return fullPath;
                    }
                    else
                    {
                        foreach (var ext in _ExecutableExtensions)
                        {
                            yield return fullPath + ext;
                        }
                    }
                }
            }
        }

        private static IEnumerable<string>/*!*/ GetAbsolutePaths(PlatformAdaptationLayer/*!*/ pal, string/*!*/ path)
        {
            if (pal.IsAbsolutePath(path))
            {
                yield return path;
            }
            else
            {
                yield return pal.GetFullPath(path);

                string var = pal.GetEnvironmentVariable("PATH");
                if (!String.IsNullOrEmpty(var))
                {
                    foreach (var prefix in var.Split(Path.PathSeparator))
                    {
                        if (prefix.Length > 0)
                        {
                            yield return Path.Combine(prefix, path);
                        }
                    }
                }

                var = Environment.GetFolderPath(Environment.SpecialFolder.System);
                if (!String.IsNullOrEmpty(var))
                {
                    yield return Path.Combine(var, path);
                }

                var = pal.GetEnvironmentVariable("SystemRoot");
                if (!String.IsNullOrEmpty(var))
                {
                    yield return Path.Combine(var, path);
                }
            }
        }


        private static bool IsShellCommand(string str)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT && Environment.OSVersion.Platform != PlatformID.Win32Windows)
			{
				return false;
			}
			switch (str.ToUpperInvariant())
			{
			case "ASSOC":
			case "BREAK":
			case "CALL":
			case "CD":
			case "CHDIR":
			case "CLS":
			case "COLOR":
			case "COPY":
			case "DATE":
			case "DEL":
			case "DIR":
			case "ECHO":
			case "ENDLOCAL":
			case "ERASE":
			case "EXIT":
			case "FOR":
			case "FTYPE":
			case "GOTO":
			case "IF":
			case "MD":
			case "MKDIR":
			case "MOVE":
			case "PATH":
			case "PAUSE":
			case "POPD":
			case "PROMPT":
			case "PUSHD":
			case "RD":
			case "REM":
			case "REN":
			case "RENAME":
			case "RMDIR":
			case "SET":
			case "SETLOCAL":
			case "SHIFT":
			case "START":
			case "TIME":
			case "TITLE":
			case "TYPE":
			case "VER":
			case "VERIFY":
			case "VOL":
				return true;
			case "MKLINK":
				return Environment.OSVersion.Version.Major >= 6;
			default:
				return false;
			}
		}

		private static MutableString JoinArguments(MutableString[] args)
		{
			MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
			for (int i = 0; i < args.Length; i++)
			{
				mutableString.Append(args[i]);
				if (args.Length > 1 && i < args.Length - 1)
				{
					mutableString.Append(' ');
				}
			}
			return mutableString;
		}

		[RubyMethod("euid", RubyMethodAttributes.PublicSingleton)]
		public static int EffectiveUserId(RubyModule self)
		{
			return 0;
		}

		[RubyMethod("kill", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("kill", RubyMethodAttributes.PublicSingleton)]
		public static object Kill(RubyModule self, object signalId, object pid)
		{
			throw RubyExceptions.CreateNotImplementedError("Signals are not currently implemented. Signal.trap just pretends to work");
		}

		[RubyMethod("pid", RubyMethodAttributes.PublicSingleton)]
		public static int GetPid(RubyModule self)
		{
			return Process.GetCurrentProcess().Id;
		}

		[RubyMethod("ppid", RubyMethodAttributes.PublicSingleton)]
		public static int GetParentPid(RubyModule self)
		{
			return 0;
		}

		[RubyMethod("times", RubyMethodAttributes.PublicSingleton)]
		public static RubyStruct GetTimes(RubyModule self)
		{
			RubyStruct rubyStruct = RubyStruct.Create(RubyStructOps.GetTmsClass(self.Context));
			try
			{
				FillTimes(rubyStruct);
				return rubyStruct;
			}
			catch (SecurityException)
			{
				RubyStructOps.TmsSetUserTime(rubyStruct, 0.0);
				RubyStructOps.TmsSetSystemTime(rubyStruct, 0.0);
				RubyStructOps.TmsSetChildUserTime(rubyStruct, 0.0);
				RubyStructOps.TmsSetChildSystemTime(rubyStruct, 0.0);
				return rubyStruct;
			}
		}

		private static void FillTimes(RubyStruct result)
		{
			Process currentProcess = Process.GetCurrentProcess();
			RubyStructOps.TmsSetUserTime(result, currentProcess.UserProcessorTime.TotalSeconds);
			RubyStructOps.TmsSetSystemTime(result, currentProcess.PrivilegedProcessorTime.TotalSeconds);
			RubyStructOps.TmsSetChildUserTime(result, 0.0);
			RubyStructOps.TmsSetChildSystemTime(result, 0.0);
		}

		[RubyMethod("uid", RubyMethodAttributes.PublicSingleton)]
		public static int UserId(RubyModule self)
		{
			return 0;
		}

		[RubyMethod("uid=", RubyMethodAttributes.PublicSingleton)]
		public static void SetUserId(RubyModule self, object temp)
		{
			throw new NotImplementedError("uid=() function is unimplemented on this machine");
		}

		[RubyMethod("wait", RubyMethodAttributes.PublicSingleton)]
		public static void Wait(RubyModule self)
		{
			throw new Errno.ChildError();
		}

		[RubyMethod("wait2", RubyMethodAttributes.PublicSingleton)]
		public static void Wait2(RubyModule self)
		{
			throw new Errno.ChildError();
		}

		[RubyMethod("waitall", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Waitall(RubyModule self)
		{
			return new RubyArray();
		}
	}
}
